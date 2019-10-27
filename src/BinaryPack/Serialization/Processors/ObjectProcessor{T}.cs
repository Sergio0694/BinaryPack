using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Attributes;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Processors.Collections;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for generic models
    /// </summary>
    /// <typeparam name="T">The type of items to handle during serialization and deserialization</typeparam>
    internal sealed partial class ObjectProcessor<T> : TypeProcessor<T> where T : new()
    {
        /// <summary>
        /// The collection of <see cref="MemberInfo"/> instances representing all the serializable members for type <typeparamref name="T"/>
        /// </summary>
        private static readonly IReadOnlyCollection<MemberInfo> Members = typeof(T).GetSerializableMembers();

        /// <summary>
        /// Gets the singleton <see cref="ObjectProcessor{T}"/> instance to use
        /// </summary>
        public static ObjectProcessor<T> Instance { get; } = new ObjectProcessor<T>();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            /* Perform a null check only if the type is a reference type.
             * In this case, a single byte will be written to the target stream,
             * with a value of 0 if the input item is null, and 1 otherwise. */
            if (!typeof(T).IsValueType)
            {
                // writer.Write(obj != null);
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitLoadArgument(Arguments.Write.T);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Cgt_Un);
                il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(bool)));

                // if (obj == null) return;
                Label isNotNull = il.DefineLabel();
                il.EmitLoadArgument(Arguments.Write.T);
                il.Emit(OpCodes.Brtrue_S, isNotNull);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(isNotNull);
            }

            // Properties serialization
            foreach (MemberInfo memberInfo in Members)
            {
                /* First special case, for unmanaged value types.
                 * Here we can just copy the property value directly to a
                 * local buffer of the right size, then cast it to a ReadOnlySpan<byte>
                 * span and write it to the target stream. No particular care is required. */
                if (memberInfo.GetMemberType().IsUnmanaged())
                {
                    // writer.Write(obj.Property);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(memberInfo);
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(memberInfo.GetMemberType()));
                }
                else if (memberInfo.GetMemberType().IsGenericType(typeof(IList<>)) ||
                         memberInfo.GetMemberType().IsGenericType(typeof(IReadOnlyList<>)) ||
                         memberInfo.GetMemberType().IsGenericType(typeof(ICollection<>)) ||
                         memberInfo.GetMemberType().IsGenericType(typeof(IReadOnlyCollection<>)) ||
                         memberInfo.GetMemberType().IsGenericType(typeof(IEnumerable<>)))
                {
                    /* Second special case, for generic interface enumerable types. This case only applies to properties
                     * of one of the generic interfaces mentioned above, and it includes two fast paths and a
                     * fallback path. The fast paths are for List<T> values, which are serialized with the
                     * ListProcessor<T> type, and for T[] values, which just use the ArrayProcessor<T> type.
                     * All other values fallback to the IEnumerableProcessor<T> type.
                     * Before serializing each value, we need to add a marker to indicate the actual processor
                     * that was used to serialize the property value, otherwise it wouldn't be possible to
                     * read it back later on correctly. 0 stand for either a List<T> or a T[] value, and 1
                     * indicates a generic IEnumerable<T> instance, using the IEnumerableProcessor<T> serializer. */
                    Type itemType = memberInfo.GetMemberType().GenericTypeArguments[0];
                    Label
                        isNotList = il.DefineLabel(),
                        isNotArray = il.DefineLabel(),
                        propertyHandled = il.DefineLabel();

                    // if (obj.Property is List<T> list) { }
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(memberInfo);
                    il.Emit(OpCodes.Isinst, typeof(List<>).MakeGenericType(itemType));
                    il.Emit(OpCodes.Brfalse_S, isNotList);

                    // writer.Write<byte>(ListProcessor<>.Id);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadInt32(typeof(ListProcessor<>).GetCustomAttribute<ProcessorIdAttribute>().Id);
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(byte)));

                    // ListProcessor<T>.Instance.Serializer(list, stream);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(memberInfo);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(List<>).MakeGenericType(itemType)));
                    il.Emit(OpCodes.Br_S, propertyHandled);

                    // else if (obj.Property is T[] array) { }
                    il.MarkLabel(isNotList);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(memberInfo);
                    il.Emit(OpCodes.Isinst, itemType.MakeArrayType());
                    il.Emit(OpCodes.Brfalse_S, isNotArray);

                    // writer.Write<byte>(ArrayProcessor<>.Id);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadInt32(typeof(ArrayProcessor<>).GetCustomAttribute<ProcessorIdAttribute>().Id);
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(byte)));

                    // ArrayProcessor<T>.Instance.Serializer(array, stream);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(memberInfo);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(itemType.MakeArrayType()));
                    il.Emit(OpCodes.Br_S, propertyHandled);

                    /* Here we need to make a distinction based on the type of the current property.
                     * We want to avoid the IEnumerable<T> serialization as much as possible, as that
                     * results in a larger binary file (as it needs to have a flag before each item to indicate
                     * whether or not the enumeration as completed) and requires a List<T> to be created
                     * during deserialization, which is used to accumulate the items being read.
                     * With an ICollection<T> instance instead we can serialize all the items one after the other,
                     * and deserialize to a single target T[] array. To do so, we check the type of the property:
                     * if it's either ICollection<T> or a type that is assignable to it, we just serialize the
                     * property with the ICollectionProcessor<T> type. If that's not the case, then we're out of luck and
                     * we're forced to fallback to the IEnumerableProcessor<T> type.
                     * We do this same exact procedure for the IReadOnlyCollection<T> interface as well. */
                    il.MarkLabel(isNotArray);
                    if (memberInfo.GetMemberType() == typeof(ICollection<>).MakeGenericType(itemType) ||
                        typeof(ICollection<>).MakeGenericType(itemType).IsAssignableFrom(memberInfo.GetMemberType()))
                    {
                        // writer.Write<byte>(ICollectionProcessor<>.Id);
                        il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                        il.EmitLoadInt32(typeof(ICollectionProcessor<>).GetCustomAttribute<ProcessorIdAttribute>().Id);
                        il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(byte)));

                        // ICollectionProcessor<T>.Instance.Serializer(obj.Property, stream);
                        il.EmitLoadArgument(Arguments.Write.T);
                        il.EmitReadMember(memberInfo);
                        il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                        il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(ICollection<>).MakeGenericType(itemType)));
                        il.Emit(OpCodes.Br_S, propertyHandled);
                    }
                    else if (memberInfo.GetMemberType() == typeof(IReadOnlyCollection<>).MakeGenericType(itemType) ||
                             typeof(IReadOnlyCollection<>).MakeGenericType(itemType).IsAssignableFrom(memberInfo.GetMemberType()))
                    {
                        // writer.Write<byte>(IReadOnlyCollectionProcessor<>.Id);
                        il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                        il.EmitLoadInt32(typeof(IReadOnlyCollectionProcessor<>).GetCustomAttribute<ProcessorIdAttribute>().Id);
                        il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(byte)));

                        // IReadOnlyCollectionProcessor<T>.Instance.Serializer(obj.Property, stream);
                        il.EmitLoadArgument(Arguments.Write.T);
                        il.EmitReadMember(memberInfo);
                        il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                        il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(IReadOnlyCollection<>).MakeGenericType(itemType)));
                    }

                    // writer.Write<byte>(IEnumerableProcessor<>.Id);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadInt32(typeof(IEnumerableProcessor<>).GetCustomAttribute<ProcessorIdAttribute>().Id);
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(byte)));

                    // IEnumerableProcessor<T>.Instance.Serializer(obj.Property, stream);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(memberInfo);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(IEnumerable<>).MakeGenericType(memberInfo.GetMemberType().GenericTypeArguments[0])));
                    il.MarkLabel(propertyHandled);
                }
                else if (memberInfo.GetMemberType().IsGenericType(typeof(IDictionary<,>)) ||
                         memberInfo.GetMemberType().IsGenericType(typeof(IReadOnlyDictionary<,>)))
                {
                    /* Third special case, for generic interface dictionary types. As with the enumerable
                     * interfaces, we first check whether the current property value is a Dictionary<K, V>
                     * instance. If that's the case, we use the DictionaryProcessor<TKey, TValue> type, otherwise we
                     * fallback to the IDictionaryProcessor<K, V> type. Other interfaces are not currently supported. */
                    Type[] generics = memberInfo.GetMemberType().GenericTypeArguments;
                    Label
                        isNotDictionary = il.DefineLabel(),
                        propertyHandled = il.DefineLabel();

                    // if (obj.Property is Dictionary<TKey, TValue> dictionary) { }
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(memberInfo);
                    il.Emit(OpCodes.Isinst, typeof(Dictionary<,>).MakeGenericType(generics));
                    il.Emit(OpCodes.Brfalse_S, isNotDictionary);

                    // writer.Write<byte>(DictionaryProcessor<,>.Id);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadInt32(typeof(DictionaryProcessor<,>).GetCustomAttribute<ProcessorIdAttribute>().Id);
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(byte)));

                    // DictionaryProcessor<TKey, TValue>.Instance.Serializer(dictionary, stream);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(memberInfo);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(Dictionary<,>).MakeGenericType(generics)));
                    il.Emit(OpCodes.Br_S, propertyHandled);

                    // writer.Write<byte>(IDictionaryProcessor<,>.Id);
                    il.MarkLabel(isNotDictionary);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadInt32(typeof(IDictionaryProcessor<,>).GetCustomAttribute<ProcessorIdAttribute>().Id);
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(byte)));

                    // IDictionaryProcessor<Tkey, TValue>.Instance.Serializer(obj.Property, stream);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(memberInfo);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(memberInfo.GetMemberType()));
                    il.MarkLabel(propertyHandled);
                }
                else
                {
                    /* In all other cases, from Nullable<T> types to T[] arrays, we use reflection to
                     * determine the right TypeProcessor<T> instance to use, then just invoke it
                     * after retrieving the current value of the property of that type. */
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(memberInfo);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(memberInfo.GetMemberType()));
                }
            }

            il.Emit(OpCodes.Ret);
        }

        /// <inheritdoc/>
        protected override void EmitDeserializer(ILGenerator il)
        {
            // T obj; ...;
            il.DeclareLocal(typeof(T));

            /* Initial null reference check for reference types.
             * If the first byte in the stream is 0, just return null. */
            if (!typeof(T).IsValueType)
            {
                // if (!reader.Read<bool>()) return null;
                Label isNotNull = il.DefineLabel();
                il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                il.EmitCall(KnownMembers.BinaryReader.ReadT(typeof(bool)));
                il.Emit(OpCodes.Brtrue_S, isNotNull);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(isNotNull);

                // T obj = new T();
                il.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
                il.EmitStoreLocal(Locals.Read.T);
            }
            else
            {
                // T obj = default;
                il.EmitLoadLocalAddress(Locals.Read.T);
                il.Emit(OpCodes.Initobj, typeof(T));
            }

            // Deserialize all the contained properties
            foreach (MemberInfo memberInfo in Members)
            {
                /* Just like with the serialization pass, handle each case separately.
                 * If the property is of an unmanaged type, just read the bytes from the
                 * stream and assign the target property by reinterpreting them to the right type. */
                if (memberInfo.GetMemberType().IsUnmanaged())
                {
                    // obj.Property = reader.Read<TProperty>();
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.BinaryReader.ReadT(memberInfo.GetMemberType()));
                    il.EmitWriteMember(memberInfo);
                }
                else if (memberInfo.GetMemberType().IsGenericType(typeof(IList<>)) ||
                         memberInfo.GetMemberType().IsGenericType(typeof(IReadOnlyList<>)) ||
                         memberInfo.GetMemberType().IsGenericType(typeof(ICollection<>)) ||
                         memberInfo.GetMemberType().IsGenericType(typeof(IReadOnlyCollection<>)) ||
                         memberInfo.GetMemberType().IsGenericType(typeof(IEnumerable<>)))
                {
                    /* When deserializing a property of one of these interface types, we first load
                     * a byte from the reader, which includes the id of the TypeSerializer<T> instance that
                     * was used to serialize the property value. The ids of all the processors involved
                     * are numbered in sequence and start at 0, so we can use an IL switch to avoid having
                     * a series of conditional jumps in the JITted code, saving some time. */
                    Type itemType = memberInfo.GetMemberType().GenericTypeArguments[0];
                    Label
                        list = il.DefineLabel(),
                        array = il.DefineLabel(),
                        iCollection = il.DefineLabel(),
                        iReadOnlyCollection = il.DefineLabel(),
                        iEnumerable = il.DefineLabel(),
                        end = il.DefineLabel();
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.BinaryReader.ReadT(typeof(byte)));
                    il.Emit(OpCodes.Switch, new[] { list, array, iCollection, iReadOnlyCollection });
                    il.Emit(OpCodes.Br_S, iEnumerable);

                    // case ListProcessor<T>.Id: obj.Property = ListProcessor<T>.Deserializer(ref reader);
                    il.MarkLabel(list);
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(typeof(List<>).MakeGenericType(itemType)));
                    il.EmitWriteMember(memberInfo);
                    il.Emit(OpCodes.Br_S, end);

                    // case ArrayProcessor<T>.Id: obj.Property = ArrayProcessor<T>.Deserializer(ref reader);
                    il.MarkLabel(array);
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(itemType.MakeArrayType()));
                    il.EmitWriteMember(memberInfo);
                    il.Emit(OpCodes.Br_S, end);

                    // case ICollectionProcessor<T>.Id: obj.Property = ICollectionProcessor<T>.Deserializer(ref reader);
                    il.MarkLabel(iCollection);
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(typeof(ICollection<>).MakeGenericType(itemType)));
                    il.EmitWriteMember(memberInfo);
                    il.Emit(OpCodes.Br_S, end);

                    // case IReadOnlyCollectionProcessor<T>.Id: obj.Property = IReadOnlyCollectionProcessor<T>.Deserializer(ref reader);
                    il.MarkLabel(iReadOnlyCollection);
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(typeof(IReadOnlyCollection<>).MakeGenericType(itemType)));
                    il.EmitWriteMember(memberInfo);
                    il.Emit(OpCodes.Br_S, end);

                    // default: obj.Property = IEnumerableProcessor<T>.Deserializer(ref reader);
                    il.MarkLabel(iEnumerable);
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(typeof(IEnumerable<>).MakeGenericType(itemType)));
                    il.EmitWriteMember(memberInfo);
                    il.MarkLabel(end);
                }
                else if (memberInfo.GetMemberType().IsGenericType(typeof(IDictionary<,>)) || 
                         memberInfo.GetMemberType().IsGenericType(typeof(IReadOnlyDictionary<,>)))
                {
                    // if (reader.Read<bool>() == DictionaryProcessor<,>.Id) { }
                    Type[] generics = memberInfo.GetMemberType().GenericTypeArguments;
                    Label
                        isNotDictionary = il.DefineLabel(),
                        end = il.DefineLabel();
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.BinaryReader.ReadT(typeof(byte)));
                    il.EmitLoadInt32(typeof(DictionaryProcessor<,>).GetCustomAttribute<ProcessorIdAttribute>().Id);
                    il.Emit(OpCodes.Bne_Un_S, isNotDictionary);

                    // Dictionary<TKey, TValue> dictionary = DictionaryProcessor<TKey, TValue>.Deserializer(ref reader);
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(typeof(Dictionary<,>).MakeGenericType(generics)));
                    il.Emit(OpCodes.Br_S, end);

                    // Dictionary<TKey, TValue> dictionary = IDictionaryProcessor<TKey, TValue>.Deserializer(ref reader);
                    il.MarkLabel(isNotDictionary);
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(typeof(IDictionary<,>).MakeGenericType(generics)));

                    // obj.Property = dictionary;
                    il.MarkLabel(end);
                    il.EmitWriteMember(memberInfo);
                }
                else
                {
                    // Fallback to the right TypeProcessor<T> for all other types
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(memberInfo.GetMemberType()));
                    il.EmitWriteMember(memberInfo);
                }
            }

            // return obj;
            il.EmitLoadLocal(Locals.Read.T);
            il.Emit(OpCodes.Ret);
        }
    }
}

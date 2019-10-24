using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Attributes;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Processors.Abstract;
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
            foreach (PropertyInfo property in
                from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where prop.CanRead && prop.CanWrite
                select prop)
            {
                /* First special case, for unmanaged value types.
                 * Here we can just copy the property value directly to a
                 * local buffer of the right size, then cast it to a ReadOnlySpan<byte>
                 * span and write it to the target stream. No particular care is required. */
                if (property.PropertyType.IsUnmanaged())
                {
                    // writer.Write(obj.Property);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(property.PropertyType));
                }
                else if (property.PropertyType.IsGenericType &&
                         property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    /* Second special case, for nullable value types. Here we
                     * can just delegate the serialization to the NullableProcessor<T> type. */
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(property.PropertyType));
                }
                else if (property.PropertyType == typeof(string))
                {
                    /* Third special case, for string values. Here we just need to
                     * load the string property and then invoke the string processor, which
                     * will handle all the possible cases like null values, empty strings, etc. */
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(StringProcessor.Instance.SerializerInfo.MethodInfo);
                }
                else if (property.PropertyType.IsArray)
                {
                    /* Fourth special case, for array types. Like with strings, we only need
                     * to load the property valaue and then delegate the rest of the
                     * serialization to the appropriate ArrayProcessor<T> instance, which
                     * is retrieved through reflection from the type of elements in the current property. */
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(property.PropertyType));
                }
                else if (property.PropertyType.IsGenericType &&
                         property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    /* Fifth special case, for List<T> types. In this case we just need to get
                     * the property value and leave the rest of the work to ListProcessor<T>. */
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(property.PropertyType));
                }
                else if (property.PropertyType.IsInterface &&
                         property.PropertyType.IsGenericType &&
                         (property.PropertyType.GetGenericTypeDefinition() == typeof(IList<>) ||
                          property.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>) ||
                          property.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                          property.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>) ||
                          property.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    /* Sixth special case, for generic interface types. This case only applies to properties
                     * of one of the generic interfaces mentioned above, and it includes two fast paths and a
                     * fallback path. The fast paths are for List<T> values, which are serialized with the
                     * ListProcessor<T> type, and for T[] values, which just use the ArrayProcessor<T> type.
                     * All other values fallback to the IEnumerableProcessor<T> type.
                     * Before serializing each value, we need to add a marker to indicate the actual processor
                     * that was used to serialize the property value, otherwise it wouldn't be possible to
                     * read it back later on correctly. 0 stand for either a List<T> or a T[] value, and 1
                     * indicates a generic IEnumerable<T> instance, using the IEnumerableProcessor<T> serializer. */
                    Label
                        isNotList = il.DefineLabel(),
                        fallback = il.DefineLabel(),
                        propertyHandled = il.DefineLabel();

                    // if (obj.Property is List<T> list) { }
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.Emit(OpCodes.Isinst, typeof(List<>).MakeGenericType(property.PropertyType.GenericTypeArguments[0]));
                    il.Emit(OpCodes.Brfalse_S, isNotList);

                    // writer.Write<byte>(ListProcessor<T>.Id);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadInt32(typeof(ListProcessor<>).GetCustomAttribute<ProcessorIdAttribute>().Id);
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(byte)));

                    // ListProcessor<T>.Instance.Serializer(list, stream);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(List<>).MakeGenericType(property.PropertyType.GenericTypeArguments[0])));
                    il.Emit(OpCodes.Br_S, propertyHandled);

                    // else if (obj.Property is T[] array) { }
                    il.MarkLabel(isNotList);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.Emit(OpCodes.Isinst, property.PropertyType.GenericTypeArguments[0].MakeArrayType());
                    il.Emit(OpCodes.Brfalse_S, fallback);

                    // writer.Write<byte>(ArrayProcessor<T>.Id);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadInt32(typeof(ArrayProcessor<>).GetCustomAttribute<ProcessorIdAttribute>().Id);
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(byte)));

                    // ArrayProcessor<T>.Instance.Serializer(array, stream);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(property.PropertyType.GenericTypeArguments[0].MakeArrayType()));
                    il.Emit(OpCodes.Br_S, propertyHandled);

                    // else { }
                    il.MarkLabel(fallback);

                    // writer.Write<byte>(IEnumerableProcessor<T>.Id);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadInt32(typeof(IEnumerableProcessor<>).GetCustomAttribute<ProcessorIdAttribute>().Id);
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(byte)));

                    // IEnumerableProcessor<T>.Instance.Serializer(obj.Property, stream);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(IEnumerable<>).MakeGenericType(property.PropertyType.GenericTypeArguments[0])));
                    il.MarkLabel(propertyHandled);
                }
                else
                {
                    // Just use another ObjectProcessor<T> instance for all other property types
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(property.PropertyType));
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
            foreach (PropertyInfo property in
                from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where prop.CanRead && prop.CanWrite
                select prop)
            {
                /* Just like with the serialization pass, handle each case separately.
                 * If the property is of an unmanaged type, just read the bytes from the
                 * stream and assign the target property by reinterpreting them to the right type. */
                if (property.PropertyType.IsUnmanaged())
                {
                    // obj.Property = reader.Read<TProperty>();
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.BinaryReader.ReadT(property.PropertyType));
                    il.EmitWriteMember(property);
                }
                else if (property.PropertyType.IsGenericType &&
                         property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // Invoke NullableProcessor<T> to read the T? value
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(property.PropertyType));
                    il.EmitWriteMember(property);
                }
                else if (property.PropertyType == typeof(string))
                {
                    // Invoke StringProcessor to read the string property
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(StringProcessor.Instance.DeserializerInfo.MethodInfo);
                    il.EmitWriteMember(property);
                }
                else if (property.PropertyType.IsArray)
                {
                    // Invoke ArrayProcessor<T> to read the TItem[] array
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(property.PropertyType));
                    il.EmitWriteMember(property);
                }
                else if (property.PropertyType.IsGenericType &&
                         property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    // Invoke ListProcessor<T> to read the List<T> list
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(property.PropertyType));
                    il.EmitWriteMember(property);
                }
                else if (property.PropertyType.IsInterface &&
                         property.PropertyType.IsGenericType &&
                         (property.PropertyType.GetGenericTypeDefinition() == typeof(IList<>) ||
                          property.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>) ||
                          property.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                          property.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>) ||
                          property.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    /* When deserializing a property of one of these interface types, we first load
                     * a byte from the reader, which includes the id of the TypeSerializer<T> instance that
                     * was used to serialize the property value. The ids of all the processors involved
                     * are numbered in sequence and start at 0, so we can use an IL switch to avoid having
                     * a series of conditional jumps in the JITted code, saving some time. */
                    Label
                        list = il.DefineLabel(),
                        array = il.DefineLabel(),
                        iEnumerable = il.DefineLabel(),
                        end = il.DefineLabel();
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.BinaryReader.ReadT(typeof(byte)));
                    il.Emit(OpCodes.Switch, new[] { list, array });
                    il.Emit(OpCodes.Br_S, iEnumerable);

                    // case ListProcessor<T>.Id: obj.Property = ListProcessor<T>.Deserializer(ref reader);
                    il.MarkLabel(list);
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(typeof(List<>).MakeGenericType(property.PropertyType.GenericTypeArguments[0])));
                    il.EmitWriteMember(property);
                    il.Emit(OpCodes.Br_S, end);

                    // case ArrayProcessor<T>.Id: obj.Property = ArrayProcessor<T>.Deserializer(ref reader);
                    il.MarkLabel(array);
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(property.PropertyType.GenericTypeArguments[0].MakeArrayType()));
                    il.EmitWriteMember(property);
                    il.Emit(OpCodes.Br_S, end);

                    // default: obj.Property = IEnumerableProcessor<T>.Deserializer(ref reader);
                    il.MarkLabel(iEnumerable);
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(typeof(IEnumerable<>).MakeGenericType(property.PropertyType.GenericTypeArguments[0])));
                    il.EmitWriteMember(property);
                    il.MarkLabel(end);
                }
                else
                {
                    // Fallback to another ObjectProcessor<T> for all other types
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                    il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(property.PropertyType));
                    il.EmitWriteMember(property);
                }
            }

            // return obj;
            il.EmitLoadLocal(Locals.Read.T);
            il.Emit(OpCodes.Ret);
        }
    }
}

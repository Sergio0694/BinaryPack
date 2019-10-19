using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Delegates;
using BinaryPack.Extensions;
using BinaryPack.Extensions.System.Reflection.Emit;
using BinaryPack.Serialization.Attributes;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Extensions;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for array types
    /// </summary>
    /// <typeparam name="T">The type of items in arrays to serialize and deserialize</typeparam>
    internal static class ArrayProcessor<T> where T : class, new()
    {
        /// <summary>
        /// The <see cref="DynamicMethod{T}"/> instance holding the serializer being built for arrays of type <typeparamref name="T"/>
        /// </summary>
        public static readonly DynamicMethod<BinarySerializer<T[]>> _Serializer = DynamicMethod<BinarySerializer<T[]>>.New();

        /// <summary>
        /// Gets the <see cref="BinarySerializer{T}"/> instance for arrays of the current type <typeparamref name="T"/>
        /// </summary>
        public static BinarySerializer<T[]> Serializer { get; } = BuildSerializer();

        /// <summary>
        /// The <see cref="DynamicMethod{T}"/> instance holding the deserializer being built for arrays of type <typeparamref name="T"/>
        /// </summary>
        public static readonly DynamicMethod<BinaryDeserializer<T[]>> _Deserializer = DynamicMethod<BinaryDeserializer<T[]>>.New();

        /// <summary>
        /// Gets the <see cref="BinaryDeserializer{T}"/> instance for arrays of the current type <typeparamref name="T"/>
        /// </summary>
        public static BinaryDeserializer<T[]> Deserializer { get; } = BuildDeserializer();

        /// <summary>
        /// Builds a new <see cref="BinarySerializer{T}"/> instance for the type <typeparamref name="T"/>
        /// </summary>
        [Pure]
        private static BinarySerializer<T[]> BuildSerializer()
        {
            return _Serializer.Build(il =>
            {
                il.DeclareLocalsFromType<Locals.Write>();
                il.DeclareLocal(typeof(int)); // Index 2: int i;

                // int size = obj?.Length ?? -1;
                Label
                    notNull = il.DefineLabel(),
                    lengthLoaded = il.DefineLabel();
                il.EmitLoadArgument(Arguments.Write.T);
                il.Emit(OpCodes.Brtrue_S, notNull);
                il.EmitLoadInt32(-1);
                il.Emit(OpCodes.Br_S, lengthLoaded);
                il.MarkLabel(notNull);
                il.EmitLoadArgument(Arguments.Write.T);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Conv_I4);
                il.MarkLabel(lengthLoaded);
                il.EmitStoreLocal(Locals.Write.Int);

                // byte* p = stackalloc byte[4]; *(int*)p = size;
                il.EmitStackalloc(typeof(int));
                il.EmitStoreLocal(Locals.Write.BytePtr);
                il.EmitLoadLocal(Locals.Write.BytePtr);
                il.EmitLoadLocal(Locals.Write.Int);
                il.EmitStoreToAddress(typeof(int));

                // stream.Write(new ReadOnlySpan<byte>(p, 4));
                il.EmitLoadArgument(Arguments.Write.Stream);
                il.EmitLoadLocal(Locals.Write.BytePtr);
                il.EmitLoadInt32(sizeof(int));
                il.Emit(OpCodes.Newobj, KnownMembers.ReadOnlySpan<byte>.UnsafeConstructor);
                il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Write, null);

                // for (int i = 0; i < size; i++) { }
                Label check = il.DefineLabel();
                il.EmitLoadInt32(0);
                il.EmitStoreLocal(2);
                il.Emit(OpCodes.Br_S, check);
                Label loop = il.DefineLabel();
                il.MarkLabel(loop);

                // SerializationProcessor<T>.Serializer(obj[i], stream);
                il.EmitLoadArgument(Arguments.Write.T);
                il.EmitLoadLocal(2);
                il.Emit(OpCodes.Ldelem_Ref);
                il.EmitLoadArgument(Arguments.Write.Stream);
                il.EmitCall(OpCodes.Call, SerializationProcessor<T>._Serializer.MethodInfo, null);

                // i++;
                il.EmitLoadLocal(2);
                il.EmitLoadInt32(1);
                il.Emit(OpCodes.Add);
                il.EmitStoreLocal(2);

                // Loop check
                il.MarkLabel(check);
                il.EmitLoadLocal(2);
                il.EmitLoadLocal(Locals.Write.Int);
                il.Emit(OpCodes.Blt_S, loop);
                il.Emit(OpCodes.Ret);
            });
        }

        /// <summary>
        /// Builds a new <see cref="BinaryDeserializer{T}"/> instance for the type <typeparamref name="T"/>
        /// </summary>
        [Pure]
        private static BinaryDeserializer<T[]> BuildDeserializer()
        {
            return _Deserializer.Build(il =>
            {
                // T obj; ...;
                il.DeclareLocal(typeof(T));
                foreach (Type type in typeof(Locals.Read).GetAttributes<LocalTypeAttribute>().Select(a => a.Type))
                {
                    il.DeclareLocal(type);
                }

                // Initialize T obj to either new T() or null
                il.EmitDeserializeEmptyInstanceOrNull<T>();

                // Skip the deserialization if the instance in null
                Label end = il.DefineLabel();
                il.EmitLoadLocal(Locals.Read.T);
                il.Emit(OpCodes.Brfalse_S, end);

                // Deserialize all the contained properties
                foreach (PropertyInfo property in
                    from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    where prop.CanRead && prop.CanWrite
                    select prop)
                {
                    if (property.PropertyType.IsUnmanaged()) il.EmitDeserializeUnmanagedProperty(property);
                    else if (property.PropertyType == typeof(string)) il.EmitDeserializeStringProperty(property);
                    else if (property.PropertyType.IsArray && property.PropertyType.GetElementType().IsUnmanaged()) il.EmitDeserializeUnmanagedArrayProperty(property);
                    else if (!property.PropertyType.IsValueType)
                    {
                        il.EmitLoadLocal(Locals.Read.T);
                        il.EmitLoadArgument(Arguments.Read.Stream);
                        il.EmitCall(OpCodes.Call, KnownMembers.SerializationProcessor.DeserializerInfo(property.PropertyType), null);
                        il.EmitWriteMember(property);
                    }
                    else throw new InvalidOperationException($"Property of type {property.PropertyType} not supported");
                }

                // return obj;
                il.MarkLabel(end);
                il.EmitLoadLocal(Locals.Read.T);
                il.Emit(OpCodes.Ret);
            });
        }
    }
}

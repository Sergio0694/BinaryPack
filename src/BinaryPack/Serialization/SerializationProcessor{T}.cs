using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Delegates;
using BinaryPack.Extensions;

namespace BinaryPack.Serialization
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class SerializationProcessor<T> where T : new()
    {
        /// <summary>
        /// Gets the <see cref="BinarySerializer{T}"/> instance for the current type <typeparamref name="T"/>
        /// </summary>
        public static BinarySerializer<T> Serializer { get; } = BuildSerializer();

        /// <summary>
        /// Gets the <see cref="BinaryDeserializer{T}"/> instance for the current type <typeparamref name="T"/>
        /// </summary>
        public static BinaryDeserializer<T> Deserializer { get; } = BuildDeserializer();

        /// <summary>
        /// Builds a new <see cref="BinarySerializer{T}"/> instance for the type <typeparamref name="T"/>
        /// </summary>
        [Pure]
        private static BinarySerializer<T> BuildSerializer()
        {
            return DynamicMethod<BinarySerializer<T>>.New(il =>
            {
                IEnumerable<PropertyInfo> properties =
                    from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    where prop.CanRead && prop.CanWrite
                    select prop;

                il.DeclareLocal(typeof(byte*));

                foreach (PropertyInfo property in properties)
                {
                    il.EmitStackalloc(property.PropertyType);
                    il.EmitStoreLocal(0);
                    il.EmitLoadLocal(0);
                    il.Emit(OpCodes.Ldarg_0);
                    il.EmitReadMember(property);
                    il.EmitStoreToAddress(property.PropertyType);
                    il.Emit(OpCodes.Ldarg_1);
                    il.EmitLoadLocal(0);
                    il.EmitLoadInt32(property.PropertyType.GetSize());
                    il.Emit(OpCodes.Newobj, KnownMethods.ReadOnlySpan<byte>.UnsafeConstructor);
                    il.EmitCall(OpCodes.Callvirt, KnownMethods.Stream.Write, null);
                }

                il.Emit(OpCodes.Ret);
            });
        }

        /// <summary>
        /// Builds a new <see cref="BinaryDeserializer{T}"/> instance for the type <typeparamref name="T"/>
        /// </summary>
        /// <returns></returns>
        [Pure]
        private static BinaryDeserializer<T> BuildDeserializer()
        {
            return DynamicMethod<BinaryDeserializer<T>>.New(il =>
            {
                IEnumerable<PropertyInfo> properties =
                    from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    where prop.CanRead && prop.CanWrite
                    select prop;

                il.DeclareLocal(typeof(T));
                il.DeclareLocal(typeof(Span<byte>));

                il.Emit(OpCodes.Newobj, KnownMethods.Type<T>.DefaultConstructor);
                il.EmitStoreLocal(0);

                foreach (PropertyInfo property in properties)
                {
                    il.EmitStackalloc(property.PropertyType);
                    il.EmitLoadInt32(property.PropertyType.GetSize());
                    il.Emit(OpCodes.Newobj, KnownMethods.Span<byte>.UnsafeConstructor);
                    il.EmitStoreLocal(1);
                    il.Emit(OpCodes.Ldarg_0);
                    il.EmitLoadLocal(1);
                    il.EmitCall(OpCodes.Callvirt, KnownMethods.Stream.Read, null);
                    il.Emit(OpCodes.Pop);
                    il.EmitLoadLocal(0);
                    il.Emit(OpCodes.Ldloca_S, 1);
                    il.EmitCall(OpCodes.Call, KnownMethods.Span<byte>.GetPinnableReference, null);
                    il.EmitLoadFromAddress(property.PropertyType);
                    il.EmitWriteMember(property);
                }

                il.EmitLoadLocal(0);
                il.Emit(OpCodes.Ret);
            });
        }
    }
}

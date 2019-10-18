using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Delegates;
using BinaryPack.Extensions.System.Reflection.Emit;
using BinaryPack.Helpers;
using BinaryPack.Serialization.Extensions;

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
                il.DeclareLocal(typeof(int));

                foreach (PropertyInfo property in properties)
                {
                    il.EmitSerializeStringProperty(property);
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
                il.DeclareLocal(typeof(int));

                il.Emit(OpCodes.Newobj, KnownMethods.Type<T>.DefaultConstructor);
                il.EmitStoreLocal(0);

                foreach (PropertyInfo property in properties)
                {
                    il.EmitDeserializeStringProperty(property);
                }

                il.EmitLoadLocal(0);
                il.Emit(OpCodes.Ret);
            });
        }
    }
}

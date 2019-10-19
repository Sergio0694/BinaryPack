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
    /// A <see langword="class"/> responsible for creating the serializers and deserializers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class SerializationProcessor<T> where T : new()
    {
        /// <summary>
        /// The <see cref="DynamicMethod{T}"/> instance holding the serializer being built for type <typeparamref name="T"/>
        /// </summary>
        public static readonly DynamicMethod<BinarySerializer<T>> _Serializer = DynamicMethod<BinarySerializer<T>>.New();

        /// <summary>
        /// Gets the <see cref="BinarySerializer{T}"/> instance for the current type <typeparamref name="T"/>
        /// </summary>
        public static BinarySerializer<T> Serializer { get; } = BuildSerializer();

        /// <summary>
        /// The <see cref="DynamicMethod{T}"/> instance holding the deserializer being built for type <typeparamref name="T"/>
        /// </summary>
        public static readonly DynamicMethod<BinaryDeserializer<T>> _Deserializer = DynamicMethod<BinaryDeserializer<T>>.New();

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
            return _Serializer.Build(il =>
            {
                // Local serialization variables
                foreach (Type type in typeof(Locals.Write).GetAttributes<LocalTypeAttribute>().Select(a => a.Type))
                {
                    il.DeclareLocal(type);
                }

                // Null check
                il.EmitSerializeFlagIfNull();

                // Properties serialization
                foreach (PropertyInfo property in
                    from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    where prop.CanRead && prop.CanWrite
                    select prop)
                {
                    if (property.PropertyType.IsUnmanaged()) il.EmitSerializeUnmanagedProperty(property);
                    else if (property.PropertyType == typeof(string)) il.EmitSerializeStringProperty(property);
                    else if (property.PropertyType.IsArray && property.PropertyType.GetElementType().IsUnmanaged()) il.EmitSerializeUnmanagedArrayProperty(property);
                    else throw new InvalidOperationException($"Property of type {property.PropertyType} not supported");
                }

                il.Emit(OpCodes.Ret);
            });
        }

        /// <summary>
        /// Builds a new <see cref="BinaryDeserializer{T}"/> instance for the type <typeparamref name="T"/>
        /// </summary>
        [Pure]
        private static BinaryDeserializer<T> BuildDeserializer()
        {
            return _Deserializer.Build(il =>
            {
                // T obj; ...;
                il.DeclareLocal(typeof(T));
                foreach (Type type in typeof(Locals.Read).GetAttributes<LocalTypeAttribute>().Select(a => a.Type))
                {
                    il.DeclareLocal(type);
                }

                // T obj = new T();
                il.Emit(OpCodes.Newobj, KnownMembers.Type<T>.DefaultConstructor);
                il.EmitStoreLocal(Locals.Read.T);

                foreach (PropertyInfo property in
                    from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    where prop.CanRead && prop.CanWrite
                    select prop)
                {
                    if (property.PropertyType.IsUnmanaged()) il.EmitDeserializeUnmanagedProperty(property);
                    else if (property.PropertyType == typeof(string)) il.EmitDeserializeStringProperty(property);
                    else if (property.PropertyType.IsArray && property.PropertyType.GetElementType().IsUnmanaged()) il.EmitDeserializeUnmanagedArrayProperty(property);
                    else throw new InvalidOperationException($"Property of type {property.PropertyType} not supported");
                }

                il.EmitLoadLocal(Locals.Read.T);
                il.Emit(OpCodes.Ret);
            });
        }
    }
}

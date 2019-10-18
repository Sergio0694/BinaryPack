using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Delegates;
using BinaryPack.Extensions.System.Reflection.Emit;
using BinaryPack.Helpers;
using BinaryPack.Serialization.Attributes;
using BinaryPack.Serialization.Constants;
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

                // Local serialization variables
                foreach (Type type in typeof(Locals.Write).GetAttributes<LocalTypeAttribute>().Select(a => a.Type))
                {
                    il.DeclareLocal(type);
                }

                foreach (PropertyInfo property in properties)
                {
                    if (property.PropertyType.IsUnmanaged()) il.EmitSerializeUnmanagedProperty(property);
                    else if (property.PropertyType == typeof(string)) il.EmitSerializeStringProperty(property);
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
            return DynamicMethod<BinaryDeserializer<T>>.New(il =>
            {
                IEnumerable<PropertyInfo> properties =
                    from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    where prop.CanRead && prop.CanWrite
                    select prop;

                // T obj; ...;
                il.DeclareLocal(typeof(T));
                foreach (Type type in typeof(Locals.Read).GetAttributes<LocalTypeAttribute>().Select(a => a.Type))
                {
                    il.DeclareLocal(type);
                }

                // T obj = new T();
                il.Emit(OpCodes.Newobj, KnownMethods.Type<T>.DefaultConstructor);
                il.EmitStoreLocal(Locals.Read.Obj);

                foreach (PropertyInfo property in properties)
                {
                    if (property.PropertyType.IsUnmanaged()) il.EmitDeserializeUnmanagedProperty(property);
                    else if (property.PropertyType == typeof(string)) il.EmitDeserializeStringProperty(property);
                    else throw new InvalidOperationException($"Property of type {property.PropertyType} not supported");
                }

                il.EmitLoadLocal(Locals.Read.Obj);
                il.Emit(OpCodes.Ret);
            });
        }
    }
}

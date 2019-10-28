using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Serialization.Processors;
using BinaryPack.Serialization.Processors.Collections;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from types inherited from <see cref="ArrayProcessor{T}"/>
        /// </summary>
        public static class TypeProcessor
        {
            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance for a dynamic serialization method for a given type
            /// </summary>
            /// <param name="objectType">The type of the item being handled by the requested processor</param>
            /// <param name="name">The name of property to retrieve from the processor instance</param>
            [Pure]
            private static MethodInfo GetMethodInfo(Type objectType, string name)
            {
                /* Get the right processor type for the input object type.
                 * Note that not all possible types are supported here. For instance,
                 * generic interfaces like IList<T> require special handling during
                 * the serialization and deserialization, which is not limited to
                 * just the use of a specific processor. For now, those case
                 * are just marked as not supported. */
                Type processorType = objectType switch
                {
                    _ when objectType.IsGenericType(typeof(Nullable<>)) => typeof(NullableProcessor<>).MakeGenericType(objectType.GenericTypeArguments[0]),
                    _ when objectType == typeof(string) => typeof(StringProcessor),
                    _ when objectType.IsLinearArrayType() => typeof(ArrayProcessor<>).MakeGenericType(objectType.GetElementType()),
                    _ when objectType.IsGenericType(typeof(List<>)) => typeof(ListProcessor<>).MakeGenericType(objectType.GenericTypeArguments[0]),
                    _ when objectType.IsGenericType(typeof(ICollection<>)) => typeof(ICollectionProcessor<>).MakeGenericType(objectType.GenericTypeArguments[0]),
                    _ when objectType.IsGenericType(typeof(IReadOnlyCollection<>)) => typeof(IReadOnlyCollectionProcessor<>).MakeGenericType(objectType.GenericTypeArguments[0]),
                    _ when objectType.IsGenericType(typeof(IEnumerable<>)) => typeof(IEnumerableProcessor<>).MakeGenericType(objectType.GenericTypeArguments[0]),
                    _ when objectType.IsGenericType(typeof(Dictionary<,>)) => typeof(DictionaryProcessor<,>).MakeGenericType(objectType.GenericTypeArguments),
                    _ when objectType.IsGenericType(typeof(IDictionary<,>)) => typeof(IDictionaryProcessor<,>).MakeGenericType(objectType.GenericTypeArguments),
                    _ when objectType.IsGenericType(typeof(IReadOnlyDictionary<,>)) => typeof(IReadOnlyDictionaryProcessor<,>).MakeGenericType(objectType.GenericTypeArguments),
                    _ => typeof(ObjectProcessor<>).MakeGenericType(objectType)
                };

                // Access the static TypeProcessor<T> instance to get the requested dynamic method
                PropertyInfo instanceInfo = processorType.GetProperty(nameof(ArrayProcessor<object>.Instance)); // Guaranteed to be there for all processors
                object processorInstance = instanceInfo.GetValue(null);
                FieldInfo fieldInfo = processorType.GetField(name);
                object genericMethod = fieldInfo.GetValue(processorInstance);
                PropertyInfo propertyInfo = genericMethod.GetType().GetProperty(nameof(DynamicMethod<Action>.MethodInfo));

                return (MethodInfo)propertyInfo.GetValue(genericMethod);
            }

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance for the dynamic serializer of a given type
            /// </summary>
            /// <param name="objectType">The type of the item being handled by the requested processor</param>
            [Pure]
            public static MethodInfo SerializerInfo(Type objectType) => GetMethodInfo(objectType, nameof(ArrayProcessor<object>.SerializerInfo));

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance for the dynamic deserializer of a given type
            /// </summary>
            /// <param name="objectType">The type of the item being handled by the requested processor</param>
            [Pure]
            public static MethodInfo DeserializerInfo(Type objectType) => GetMethodInfo(objectType, nameof(ArrayProcessor<object>.DeserializerInfo));
        }
    }
}
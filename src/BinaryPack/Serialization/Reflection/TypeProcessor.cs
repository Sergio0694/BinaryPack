using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Serialization.Processors;

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
                Type processorType;
                if (objectType.IsGenericType &&
                    objectType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    Type structType = objectType.GenericTypeArguments[0];
                    processorType = typeof(NullableProcessor<>).MakeGenericType(structType);
                }
                else if (objectType == typeof(string))
                {
                    processorType = typeof(StringProcessor);
                }
                else if (objectType.IsArray &&
                         objectType == objectType.GetElementType().MakeArrayType())
                {
                    Type elementType = objectType.GetElementType();
                    processorType = typeof(ArrayProcessor<>).MakeGenericType(elementType);
                }
                else if (objectType.IsGenericType &&
                            objectType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type itemType = objectType.GenericTypeArguments[0];
                    processorType = typeof(ListProcessor<>).MakeGenericType(itemType);
                }
                else processorType = typeof(ObjectProcessor<>).MakeGenericType(objectType);

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
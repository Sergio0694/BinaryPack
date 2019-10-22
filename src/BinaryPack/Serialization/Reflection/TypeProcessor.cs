using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using BinaryPack.Extensions.System.Reflection.Emit;
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
            /// <param name="processorType">The type of processor to target, must be a generic version of a type inheriting from <see cref="Processors.Abstract.TypeProcessor{T}"/></param>
            /// <param name="objectType">The type of the item being handled by the requested processor</param>
            /// <param name="name">The name of property to retrieve from the processor instance</param>
            [Pure]
            private static MethodInfo GetMethodInfo(Type processorType, Type objectType, string name)
            {
                Type genericType = processorType.MakeGenericType(objectType);
                PropertyInfo instanceInfo = genericType.GetProperty(nameof(ArrayProcessor<object>.Instance)); // Guaranteed to be there for all processors
                object processorInstance = instanceInfo.GetValue(null);
                FieldInfo fieldInfo = genericType.GetField(name);
                object genericMethod = fieldInfo.GetValue(processorInstance);
                PropertyInfo propertyInfo = genericMethod.GetType().GetProperty(nameof(DynamicMethod<Action>.MethodInfo));

                return (MethodInfo)propertyInfo.GetValue(genericMethod);
            }

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance for the dynamic serializer of a given type
            /// </summary>
            /// <param name="processorType">The type of processor to target, must be a generic version of a type inheriting from <see cref="Processors.Abstract.TypeProcessor{T}"/></param>
            /// <param name="objectType">The type of the item being handled by the requested processor</param>
            [Pure]
            public static MethodInfo SerializerInfo(Type processorType, Type objectType) => GetMethodInfo(processorType, objectType, nameof(ArrayProcessor<object>.SerializerInfo));

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance for the dynamic deserializer of a given type
            /// </summary>
            /// <param name="processorType">The type of processor to target, must be a generic version of a type inheriting from <see cref="Processors.Abstract.TypeProcessor{T}"/></param>
            /// <param name="objectType">The type of the item being handled by the requested processor</param>
            [Pure]
            public static MethodInfo DeserializerInfo(Type processorType, Type objectType) => GetMethodInfo(processorType, objectType, nameof(ArrayProcessor<object>.DeserializerInfo));
        }
    }
}
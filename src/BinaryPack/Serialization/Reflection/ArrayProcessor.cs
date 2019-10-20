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
        /// A <see langword="class"/> containing methods from the <see cref="ArrayProcessor{T}"/> type
        /// </summary>
        public static class ArrayProcessor
        {
            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance for a dynamic serialization method for a given type
            /// </summary>
            /// <param name="type">The type of object to look up the serialization method for</param>
            /// <param name="name">The name of the method to retrieve</param>
            [Pure]
            private static MethodInfo GetMethodInfo(Type type, string name)
            {
                Type processorType = typeof(ArrayProcessor<>).MakeGenericType(type);
                PropertyInfo instanceInfo = processorType.GetProperty(nameof(ArrayProcessor<object>.Instance), BindingFlags.Public | BindingFlags.Static);
                object processorInstance = instanceInfo.GetValue(null);
                FieldInfo fieldInfo = processorType.GetField(name, BindingFlags.Public | BindingFlags.Static);
                object genericMethod = fieldInfo.GetValue(processorInstance);
                PropertyInfo propertyInfo = genericMethod.GetType().GetProperty(nameof(DynamicMethod<Action>.MethodInfo));

                return (MethodInfo)propertyInfo.GetValue(genericMethod);
            }

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance for the dynamic serializer of a given type
            /// </summary>
            /// <param name="type">The type of object to look up the serializer for</param>
            [Pure]
            public static MethodInfo SerializerInfo(Type type) => GetMethodInfo(type, nameof(ArrayProcessor<object>._Serializer));

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance for the dynamic deserializer of a given type
            /// </summary>
            /// <param name="type">The type of object to look up the deserializer for</param>
            [Pure]
            public static MethodInfo DeserializerInfo(Type type) => GetMethodInfo(type, nameof(ArrayProcessor<object>._Deserializer));
        }
    }
}
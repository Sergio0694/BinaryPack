using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {

        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="Unsafe"/> type
        /// </summary>
        /// <typeparam name="T">The generic type argument for generic methods</typeparam>
        public static class Unsafe<T>
        {
            // Mapping of methods for target type not known at compile time
            private static readonly Dictionary<Type, MethodInfo> _AsMap = new Dictionary<Type, MethodInfo>();

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="System.Runtime.CompilerServices.Unsafe.As{TFrom,TTo}"/> method
            /// </summary>
            public static MethodInfo As(Type type)
            {
                if (!_AsMap.TryGetValue(type, out MethodInfo methodInfo))
                {
                    methodInfo = (
                        from method in typeof(System.Runtime.CompilerServices.Unsafe).GetMethods()
                        where method.Name.Equals(nameof(System.Runtime.CompilerServices.Unsafe.As)) &&
                              method.GetGenericArguments().Length == 2
                        let target = method.MakeGenericMethod(typeof(T), type)
                        select target).First();
                    _AsMap.Add(type, methodInfo);
                }

                return methodInfo;
            }
        }
    }
}

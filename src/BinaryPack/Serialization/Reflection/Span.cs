using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="System.Span{T}"/> type
        /// </summary>
        public static class Span
        {
            /// <summary>
            /// Gets the <see cref="Span{T}"/> constructor that takes a generic array
            /// </summary>
            [Pure]
            public static ConstructorInfo ArrayConstructor(Type type) => (
                from ctor in typeof(Span<>).MakeGenericType(type).GetConstructors()
                let args = ctor.GetParameters()
                where args.Length == 1 &&
                      args[0].ParameterType == type.MakeArrayType()
                select ctor).First();

            /// <summary>
            /// Gets the <see cref="Span{T}"/> constructor that takes a <see langword="void"/> pointer and a size
            /// </summary>
            [Pure]
            public static ConstructorInfo UnsafeConstructor(Type type) => (
                from ctor in typeof(Span<>).MakeGenericType(type).GetConstructors()
                let args = ctor.GetParameters()
                where args.Length == 2 &&
                      args[0].ParameterType == typeof(void*) &&
                      args[1].ParameterType == typeof(int)
                select ctor).First();

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="Span{T}.GetPinnableReference"/> method
            /// </summary>
            [Pure]
            public static MethodInfo GetPinnableReference(Type type) => (
                from method in typeof(Span<>).MakeGenericType(type).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                where method.Name.Equals(nameof(Span<byte>.GetPinnableReference))
                select method).First();
        }
    }
}
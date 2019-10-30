using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="Span{T}"/> type
        /// </summary>
        public static class Span
        {
            /// <summary>
            /// Gets the <see cref="Span{T}"/> constructor that takes a generic array
            /// </summary>
            [Pure]
            public static ConstructorInfo ArrayConstructor(Type type) => typeof(Span<>).MakeGenericType(type).GetConstructor(new[] { type.MakeArrayType() });

            /// <summary>
            /// Gets the <see cref="Span{T}"/> constructor that takes a generic array, a start index and the length
            /// </summary>
            /// <param name="type">The type parameter to use for the target <see cref="Span{T}"/> type to use</param>
            [Pure]
            public static ConstructorInfo ArrayWithOffsetAndLengthConstructor(Type type) => typeof(Span<>).MakeGenericType(type).GetConstructor(new[] { type.MakeArrayType(), typeof(int), typeof(int) });

            /// <summary>
            /// Gets the <see cref="Span{T}"/> constructor that takes a <see langword="void"/> pointer and a size
            /// </summary>
            [Pure]
            public static ConstructorInfo UnsafeConstructor(Type type) => typeof(Span<>).MakeGenericType(type).GetConstructor(new[] { typeof(void*), typeof(int) });

            /// <summary>
            /// Gets the <see cref="Span{T}"/> constructor (actually, a <see cref="MethodInfo"/> instance) that takes a <see langword="ref"/> and a size
            /// </summary>
            /// <param name="type">The type parameter to use for the target <see cref="Span{T}"/> type to use</param>
            public static MethodInfo RefConstructor(Type type) => typeof(MemoryMarshal).GetMethod(nameof(MemoryMarshal.CreateSpan)).MakeGenericMethod(type);
        }
    }
}

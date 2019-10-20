using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    /// <summary>
    /// A <see langword="class"/> exposing some frequently used members for quick lookup
    /// </summary>
    internal static partial class KnownMembers
    {
        /// <summary>
        /// A <see langword="class"/> containing members from the <see cref="Array"/> type
        /// </summary>
        public static class Array
        {
            /// <summary>
            /// The <see cref="MethodInfo"/> instance mapping the <see cref="System.Array.Empty{T}"/> method
            /// </summary>
            private static readonly MethodInfo _Empty  = typeof(System.Array).GetMethod(nameof(System.Array.Empty), BindingFlags.Public | BindingFlags.Static);

            /// <summary>
            /// Gets a generic <see cref="MethodInfo"/> instance mapping the <see cref="System.Runtime.InteropServices.MemoryMarshal.AsBytes{T}(System.ReadOnlySpan{T})"/> method
            /// </summary>
            [Pure]
            public static MethodInfo Empty(Type type) => _Empty.MakeGenericMethod(type);
        }
    }
}

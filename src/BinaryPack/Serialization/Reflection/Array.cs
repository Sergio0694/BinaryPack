using System;
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
            /// Gets the <see cref="PropertyInfo"/> instance mapping the <see cref="System.Array.Length"/> property
            /// </summary>
            public static PropertyInfo Length { get; } = typeof(System.Array).GetProperty(nameof(System.Array.Length));

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="System.Array.Empty{T}"/> method
            /// </summary>
            private static MethodInfo _Empty { get; } = typeof(System.Array).GetMethod(nameof(System.Array.Empty), BindingFlags.Public | BindingFlags.Static);

            /// <summary>
            /// Gets a generic <see cref="MethodInfo"/> instance mapping the <see cref="System.Runtime.InteropServices.MemoryMarshal.AsBytes{T}(System.ReadOnlySpan{T})"/> method
            /// </summary>
            public static MethodInfo Empty(Type type) => _Empty.MakeGenericMethod(type);
        }
    }
}

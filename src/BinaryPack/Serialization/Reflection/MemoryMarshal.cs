using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="Unsafe"/> type
        /// </summary>
        public static class MemoryMarshal
        {
            /// <summary>
            /// The <see cref="MethodInfo"/> instance mapping the <see cref="System.Runtime.InteropServices.MemoryMarshal.AsBytes{T}(Span{T})"/> method
            /// </summary>
            private static readonly MethodInfo _AsByteSpan = (
                from method in typeof(System.Runtime.InteropServices.MemoryMarshal).GetMethods(BindingFlags.Public | BindingFlags.Static)
                where method.Name.Equals(nameof(System.Runtime.InteropServices.MemoryMarshal.AsBytes)) &&
                      method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Span<>)
                select method).First();

            /// <summary>
            /// Gets a generic <see cref="MethodInfo"/> instance mapping the <see cref="System.Runtime.InteropServices.MemoryMarshal.AsBytes{T}(Span{T})"/> method
            /// </summary>
            [Pure]
            public static MethodInfo AsByteSpan(Type type) => _AsByteSpan.MakeGenericMethod(type);

            /// <summary>
            /// The <see cref="MethodInfo"/> instance mapping the <see cref="System.Runtime.InteropServices.MemoryMarshal.AsBytes{T}(ReadOnlySpan{T})"/> method
            /// </summary>
            private static readonly MethodInfo _AsByteReadOnlySpan = (
                from method in typeof(System.Runtime.InteropServices.MemoryMarshal).GetMethods(BindingFlags.Public | BindingFlags.Static)
                where method.Name.Equals(nameof(System.Runtime.InteropServices.MemoryMarshal.AsBytes)) &&
                      method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(ReadOnlySpan<>)
                select method).First();

            /// <summary>
            /// Gets a generic <see cref="MethodInfo"/> instance mapping the <see cref="System.Runtime.InteropServices.MemoryMarshal.AsBytes{T}(ReadOnlySpan{T})"/> method
            /// </summary>
            [Pure]
            public static MethodInfo AsByteReadOnlySpan(Type type) => _AsByteReadOnlySpan.MakeGenericMethod(type);
        }
    }
}

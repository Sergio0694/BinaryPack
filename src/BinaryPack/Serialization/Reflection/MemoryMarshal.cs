using System;
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
            /// The <see cref="MethodInfo"/> instance mapping the <see cref="System.Runtime.InteropServices.MemoryMarshal.AsBytes{T}(System.Span{T})"/> method
            /// </summary>
            private static readonly MethodInfo _AsByteSpan = (
                from method in typeof(System.Runtime.InteropServices.MemoryMarshal).GetMethods(BindingFlags.Public | BindingFlags.Static)
                where method.Name.Equals(nameof(System.Runtime.InteropServices.MemoryMarshal.AsBytes)) &&
                      method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(System.Span<>)
                select method).First();

            /// <summary>
            /// Gets a generic <see cref="MethodInfo"/> instance mapping the <see cref="System.Runtime.InteropServices.MemoryMarshal.AsBytes{T}(System.Span{T})"/> method
            /// </summary>
            public static MethodInfo AsByteSpan(Type type) => _AsByteSpan.MakeGenericMethod(type);

            /// <summary>
            /// The <see cref="MethodInfo"/> instance mapping the <see cref="System.Runtime.InteropServices.MemoryMarshal.AsBytes{T}(System.ReadOnlySpan{T})"/> method
            /// </summary>
            private static readonly MethodInfo _AsByteReadOnlySpan = (
                from method in typeof(System.Runtime.InteropServices.MemoryMarshal).GetMethods(BindingFlags.Public | BindingFlags.Static)
                where method.Name.Equals(nameof(System.Runtime.InteropServices.MemoryMarshal.AsBytes)) &&
                      method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(System.ReadOnlySpan<>)
                select method).First();

            /// <summary>
            /// Gets a generic <see cref="MethodInfo"/> instance mapping the <see cref="System.Runtime.InteropServices.MemoryMarshal.AsBytes{T}(System.ReadOnlySpan{T})"/> method
            /// </summary>
            public static MethodInfo AsByteReadOnlySpan(Type type) => _AsByteReadOnlySpan.MakeGenericMethod(type);
        }
    }
}

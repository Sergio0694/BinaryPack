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
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="System.Runtime.InteropServices.MemoryMarshal.AsBytes{T}(System.ReadOnlySpan{T})"/> method
            /// </summary>
            private static MethodInfo _AsBytes { get; } = (
                from method in typeof(System.Runtime.InteropServices.MemoryMarshal).GetMethods(BindingFlags.Public | BindingFlags.Static)
                where method.Name.Equals(nameof(System.Runtime.InteropServices.MemoryMarshal.AsBytes)) &&
                      method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(System.ReadOnlySpan<>)
                select method).First();

            /// <summary>
            /// Gets a generic <see cref="MethodInfo"/> instance mapping the <see cref="System.Runtime.InteropServices.MemoryMarshal.AsBytes{T}(System.ReadOnlySpan{T})"/> method
            /// </summary>
            public static MethodInfo AsBytes(Type type) => _AsBytes.MakeGenericMethod(type);
        }
    }
}

using System;
using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {
        /// <summary>
        /// A <see langword="class"/> containing members from the <see cref="string"/> type
        /// </summary>
        public static class String
        {
            /// <summary>
            /// Gets the <see cref="PropertyInfo"/> instance mapping the <see cref="string.Length"/> property
            /// </summary>
            public static PropertyInfo Length { get; } = typeof(string).GetProperty(nameof(string.Length));

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="MemoryExtensions.AsSpan(string)"/> method
            /// </summary>
            public static MethodInfo AsSpan { get; } = (
                from method in typeof(MemoryExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                where method.Name.Equals(nameof(MemoryExtensions.AsSpan))
                let args = method.GetParameters()
                where args.Length == 1 &&
                      args[0].ParameterType == typeof(string)
                select method).First();
        }
    }
}

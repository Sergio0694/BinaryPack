using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {
        /// <summary>
        /// A <see langword="class"/> containing members from the <see cref="Encoding"/> type
        /// </summary>
        public static class Encoding
        {
            /// <summary>
            /// Gets the <see cref="PropertyInfo"/> instance mapping the <see cref="System.Text.Encoding.UTF8"/> property
            /// </summary>
            public static PropertyInfo UTF8 { get; } = typeof(System.Text.Encoding).GetProperty(nameof(System.Text.Encoding.UTF8));

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="System.Text.Encoding.GetByteCount(System.ReadOnlySpan{char})"/> method
            /// </summary>
            [Pure]
            public static MethodInfo GetByteCount { get; } = (
                from method in typeof(System.Text.Encoding).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                where method.Name.Equals(nameof(System.Text.Encoding.GetByteCount))
                let args = method.GetParameters()
                where args.Length == 1 &&
                      args[0].ParameterType == typeof(System.ReadOnlySpan<char>)
                select method).First();

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="System.Text.Encoding.GetBytes(System.ReadOnlySpan{char},System.Span{byte})"/> method
            /// </summary>
            [Pure]
            public static MethodInfo GetBytes { get; } = (
                from method in typeof(System.Text.Encoding).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                where method.Name.Equals(nameof(System.Text.Encoding.GetBytes))
                let args = method.GetParameters()
                where args.Length == 2 &&
                      args[0].ParameterType == typeof(System.ReadOnlySpan<char>) &&
                      args[1].ParameterType == typeof(System.Span<byte>)
                select method).First();

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="System.Text.Encoding.GetString(byte*,int)"/> method
            /// </summary>
            [Pure]
            public static MethodInfo GetString { get; } = (
                from method in typeof(System.Text.Encoding).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                where method.Name.Equals(nameof(System.Text.Encoding.GetString))
                let args = method.GetParameters()
                where args.Length == 2 &&
                      args[0].ParameterType == typeof(byte*) &&
                      args[1].ParameterType == typeof(int)
                select method).First();
        }
    }
}

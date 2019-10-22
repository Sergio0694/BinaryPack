using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// A <see langword="class"/> that provides extension methods for the <see cref="Type"/> type
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Gets the syze in bytes of the given type
        /// </summary>
        /// <param name="type">The input type to analyze</param>
        [Pure]
        public static int GetSize(this Type type) => (int)typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf)).MakeGenericMethod(type).Invoke(null, null);

        /// <summary>
        /// Helper <see langword="class"/> for the <see cref="IsUnmanaged"/> method
        /// </summary>
        /// <typeparam name="T">The type to check against the <see langword="unmanaged"/> constraint</typeparam>
        private static class _IsUnmanaged<T> where T : unmanaged { }

        /// <summary>
        /// Checks whether or not the input type respects the <see langword="unmanaged"/> constraint
        /// </summary>
        /// <param name="type">The input type to analyze</param>
        [Pure]
        public static bool IsUnmanaged(this Type type)
        {
            try
            {
                _ = typeof(_IsUnmanaged<>).MakeGenericType(type);
                return true;
            }
            catch (ArgumentException)
            {
                // Not unmanaged
                return false;
            }
        }
    }
}

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
        }
    }
}

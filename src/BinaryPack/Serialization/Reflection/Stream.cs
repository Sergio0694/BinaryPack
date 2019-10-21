using System;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="Stream"/> type
        /// </summary>
        public static class Stream
        {
            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="System.IO.Stream.Read(Span{byte})"/> method
            /// </summary>
            public static MethodInfo Read { get; } = typeof(System.IO.Stream).GetMethod(nameof(System.IO.Stream.Read), new[] { typeof(Span<byte>) });

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="System.IO.Stream.Write(ReadOnlySpan{byte})"/> method
            /// </summary>
            public static MethodInfo Write { get; } = typeof(System.IO.Stream).GetMethod(nameof(System.IO.Stream.Write), new[] { typeof(ReadOnlySpan<byte>) });
        }
    }
}

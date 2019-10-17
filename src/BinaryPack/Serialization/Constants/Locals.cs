namespace BinaryPack.Serialization.Constants
{
    /// <summary>
    /// A <see langword="class"/> that exposes hardcoded indices for local variables
    /// </summary>
    internal static class Locals
    {
        /// <summary>
        /// A <see langword="class"/> with a collection of indices for locals used during deserialization
        /// </summary>
        public static class Read
        {
            /// <summary>
            /// The index of the <typeparam name="T">T</typeparam> local variable
            /// </summary>
            public const int T = 0;

            /// <summary>
            /// The index of the <see cref="byte"/> <see cref="System.Span{T}"/> local variable
            /// </summary>
            public const int SpanByte = 1;
        }

        /// <summary>
        /// A <see langword="class"/> with a collection of indices for locals used during serialization
        /// </summary>
        public static class Write
        {
            /// <summary>
            /// The index of the <see cref="byte"/>* local variable
            /// </summary>
            public const int BytePtr = 0;

            /// <summary>
            /// The index of the <see cref="int"/> local variable
            /// </summary>
            public const int Int = 1;
        }
    }
}

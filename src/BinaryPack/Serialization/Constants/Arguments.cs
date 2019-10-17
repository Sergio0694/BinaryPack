namespace BinaryPack.Serialization.Constants
{
    /// <summary>
    /// A <see langword="class"/> that exposes hardcoded indices for arguments
    /// </summary>
    internal static class Arguments
    {
        /// <summary>
        /// A <see langword="class"/> with a collection of indices for arguments used during deserialization
        /// </summary>
        public static class Read
        {
            /// <summary>
            /// The index of the target <see cref="System.IO.Stream"/> instance
            /// </summary>
            public const int Stream = 0;
        }

        /// <summary>
        /// A <see langword="class"/> with a collection of indices for arguments used during serialization
        /// </summary>
        public static class Write
        {
            /// <summary>
            /// The index of the input instance
            /// </summary>
            public const int Obj = 0;

            /// <summary>
            /// The index of the input <see cref="System.IO.Stream"/> instance
            /// </summary>
            public const int Stream = 1;
        }
    }
}

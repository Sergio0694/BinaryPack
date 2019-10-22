namespace BinaryPack.Serialization.Constants
{
    /// <summary>
    /// A <see langword="class"/> that exposes hardcoded indices for arguments
    /// </summary>
    internal static class Arguments
    {
        /// <summary>
        /// An <see langword="enum"/> with a collection of indices for arguments used during deserialization
        /// </summary>
        public enum Read
        {
            /// <summary>
            /// The source <see cref="Buffers.BinaryReader"/> instance
            /// </summary>
            RefBinaryReader
        }

        /// <summary>
        /// An <see langword="enum"/> with a collection of indices for arguments used during serialization
        /// </summary>
        public enum Write
        {
            /// <summary>
            /// The input instance of a given type
            /// </summary>
            T,

            /// <summary>
            /// The <see langword="ref"/> to the target <see cref="Buffers.BinaryWriter"/> instance
            /// </summary>
            RefBinaryWriter
        }
    }
}

using BinaryPack.Serialization.Attributes;

namespace BinaryPack.Serialization.Processors
{
    internal sealed partial class StringProcessor
    {
        /// <summary>
        /// A <see langword="class"/> that exposes local variables for <see cref="StringProcessor"/>
        /// </summary>
        private static class Locals
        {
            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during serialization
            /// </summary>
            public enum Write
            {
                /// <summary>
                /// The <see cref="byte"/>* local variable
                /// </summary>
                [LocalType(typeof(byte*))]
                BytePtr,

                /// <summary>
                /// The <see cref="int"/> local variable to track the length of the source <see cref="string"/>
                /// </summary>
                [LocalType(typeof(int))]
                Length
            }

            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during deserialization
            /// </summary>
            public enum Read
            {
                /// <summary>
                /// The <see cref="byte"/> <see cref="System.Span{T}"/> local variable
                /// </summary>
                [LocalType(typeof(System.Span<byte>))]
                SpanByte,

                /// <summary>
                /// The <see cref="int"/> local variable to track the length of the target <see cref="string"/>
                /// </summary>
                [LocalType(typeof(int))]
                Length
            }
        }
    }
}

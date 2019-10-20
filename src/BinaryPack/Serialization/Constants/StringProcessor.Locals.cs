using BinaryPack.Serialization.Attributes;

namespace BinaryPack.Serialization.Processors
{
    internal sealed partial class StringProcessor
    {
        /// <summary>
        /// A <see langword="class"/> that exposes hardcoded indices for local variables for <see cref="StringProcessor"/>
        /// </summary>
        private static class Locals
        {
            /// <summary>
            /// An <see langword="enum"/> with a collection of indices for locals used during serialization
            /// </summary>
            public enum Write
            {
                /// <summary>
                /// The index of the <see cref="byte"/>* local variable
                /// </summary>
                [LocalType(typeof(byte*))]
                BytePtr,

                /// <summary>
                /// The index of the <see cref="int"/> local variable to track the length of the source <see cref="string"/>
                /// </summary>
                [LocalType(typeof(int))]
                Length
            }

            /// <summary>
            /// An <see langword="enum"/> with a collection of indices for locals used during deserialization
            /// </summary>
            public enum Read
            {
                /// <summary>
                /// The index of the <see cref="byte"/> <see cref="System.Span{T}"/> local variable
                /// </summary>
                [LocalType(typeof(System.Span<byte>))]
                SpanByte,

                /// <summary>
                /// The index of the <see cref="int"/> local variable to track the length of the target <see cref="string"/>
                /// </summary>
                [LocalType(typeof(int))]
                Length
            }
        }
    }
}

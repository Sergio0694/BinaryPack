using BinaryPack.Serialization.Attributes;

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
            /// The index of the input instance
            /// </summary>
            public const int T = 0;

            /// <summary>
            /// The index of the <see cref="byte"/> <see cref="System.Span{T}"/> local variable
            /// </summary>
            [LocalType(typeof(System.Span<byte>))]
            public const int SpanByte = 1;

            /// <summary>
            /// The index of the <see cref="int"/> local variable
            /// </summary>
            [LocalType(typeof(int))]
            public const int Int = 2;

            /// <summary>
            /// The index of an <see cref="object"/> local variable to store reference values
            /// </summary>
            [LocalType(typeof(object))]
            public const int Obj = 3;
        }

        /// <summary>
        /// A <see langword="class"/> with a collection of indices for locals used during serialization
        /// </summary>
        public static class Write
        {
            /// <summary>
            /// The index of the <see cref="byte"/>* local variable
            /// </summary>
            [LocalType(typeof(byte*))]
            public const int BytePtr = 0;

            /// <summary>
            /// The index of the <see cref="int"/> local variable
            /// </summary>
            [LocalType(typeof(int))]
            public const int Int = 1;
        }
    }
}

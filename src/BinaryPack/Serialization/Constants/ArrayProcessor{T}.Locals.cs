using BinaryPack.Serialization.Attributes;

namespace BinaryPack.Serialization
{
    internal static partial class ArrayProcessor<T>
    {
        /// <summary>
        /// A <see langword="class"/> that exposes hardcoded indices for local variables for <see cref="ArrayProcessor{T}"/>
        /// </summary>
        private static class Locals
        {
            /// <summary>
            /// A <see langword="class"/> with a collection of indices for locals used during serialization
            /// </summary>
            public sealed class Write
            {
                /// <summary>
                /// The index of the <see cref="byte"/>* local variable
                /// </summary>
                [LocalType(typeof(byte*))]
                public const int BytePtr = 0;

                /// <summary>
                /// The index of the <see cref="int"/> local variable to track the length of the source <typeparamref name="T"/> array
                /// </summary>
                [LocalType(typeof(int))]
                public const int Length = 1;

                /// <summary>
                /// The index of the <see cref="int"/> local variable for the loop counter
                /// </summary>
                [LocalType(typeof(int))]
                public const int I = 2;
            }

            /// <summary>
            /// A <see langword="class"/> with a collection of indices for locals used during deserialization
            /// </summary>
            public sealed class Read
            {
                /// <summary>
                /// The index of the target <typeparamref name="T"/> array
                /// </summary>
                public const int Array = 0;

                /// <summary>
                /// The index of the <see cref="byte"/> <see cref="System.Span{T}"/> local variable
                /// </summary>
                [LocalType(typeof(System.Span<byte>))]
                public const int SpanByte = 1;

                /// <summary>
                /// The index of the <see cref="int"/> local variable to track the length of the target <typeparamref name="T"/> array
                /// </summary>
                [LocalType(typeof(int))]
                public const int Length = 2;

                /// <summary>
                /// The index of the <see cref="int"/> local variable for the loop counter
                /// </summary>
                [LocalType(typeof(int))]
                public const int I = 3;
            }
        }
    }
}

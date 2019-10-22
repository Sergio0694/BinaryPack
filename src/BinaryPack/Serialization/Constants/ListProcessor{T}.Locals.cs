using BinaryPack.Attributes;

namespace BinaryPack.Serialization.Processors
{
    internal sealed partial class ListProcessor<T>
    {
        /// <summary>
        /// A <see langword="class"/> that exposes hardcoded indices for local variables for <see cref="ListProcessor{T}"/>
        /// </summary>
        private static class Locals
        {
            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during serialization
            /// </summary>
            public enum Write
            {
                /// <summary>
                /// The <see cref="int"/> local variable to track the count of the source <see cref="System.Collections.Generic.List{T}"/> instance
                /// </summary>
                [LocalType(typeof(int))]
                Count,

                /// <summary>
                /// The <see cref="int"/> local variable for the loop counter
                /// </summary>
                [LocalType(typeof(int))]
                I,

                /// <summary>
                /// The <see langword="ref"/> <typeparamref name="T"/> variable, used to iterate arrays of reference types
                /// </summary>
                RefT
            }

            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during deserialization
            /// </summary>
            public enum Read
            {
                /// <summary>
                /// The target <see cref="System.Collections.Generic.List{T}"/> instance
                /// </summary>
                ListT,

                /// <summary>
                /// The target array of type <typeparamref name="T"/> to load and inject
                /// </summary>
                ArrayT,

                /// <summary>
                /// The <see cref="byte"/> <see cref="System.Span{T}"/> local variable
                /// </summary>
                [LocalType(typeof(System.Span<byte>))]
                SpanByte,

                /// <summary>
                /// The <see cref="int"/> local variable to track the count of the target <see cref="System.Collections.Generic.List{T}"/>
                /// </summary>
                [LocalType(typeof(int))]
                Count,

                /// <summary>
                /// The <see cref="int"/> local variable for the loop counter
                /// </summary>
                [LocalType(typeof(int))]
                I
            }
        }
    }
}


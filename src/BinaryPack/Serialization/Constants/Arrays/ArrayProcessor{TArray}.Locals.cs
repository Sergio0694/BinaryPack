using BinaryPack.Attributes;

namespace BinaryPack.Serialization.Processors.Arrays
{
    internal sealed partial class ArrayProcessor<TArray>
    {
        /// <summary>
        /// A <see langword="class"/> that exposes hardcoded indices for local variables for <see cref="ArrayProcessor{T}"/>
        /// </summary>
        private static class Locals
        {
            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during serialization
            /// </summary>
            public enum Write
            {
                /// <summary>
                /// The <see cref="int"/> local variable to track the length of the source <typeparamref name="TArray"/> array
                /// </summary>
                [LocalType(typeof(int))]
                Length,

                /// <summary>
                /// The <see cref="int"/> local variable for the loop counter
                /// </summary>
                [LocalType(typeof(int))]
                I,

                /// <summary>
                /// The <see langword="ref"/> <typeparamref name="TArray"/> variable, used to iterate arrays of reference types
                /// </summary>
                RefT
            }

            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during deserialization
            /// </summary>
            public enum Read
            {
                /// <summary>
                /// The target <typeparamref name="TArray"/> array
                /// </summary>
                ArrayT,

                /// <summary>
                /// The <see cref="int"/> local variable to track the length of the target <typeparamref name="TArray"/> array
                /// </summary>
                [LocalType(typeof(int))]
                Length,

                /// <summary>
                /// The <see cref="int"/> local variable for the loop counter
                /// </summary>
                [LocalType(typeof(int))]
                I,

                /// <summary>
                /// The <see langword="ref"/> <typeparamref name="TArrayT"/> variable, used to iterate arrays of reference types
                /// </summary>
                RefT
            }
        }
    }
}

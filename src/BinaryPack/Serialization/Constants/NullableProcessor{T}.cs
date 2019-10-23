using BinaryPack.Attributes;

namespace BinaryPack.Serialization.Processors
{
    internal sealed partial class NullableProcessor<T>
    {
        /// <summary>
        /// A <see langword="class"/> that exposes hardcoded indices for local variables for <see cref="NullableProcessor{T}"/>
        /// </summary>
        private static class Locals
        {
            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during serialization
            /// </summary>
            public enum Write
            {
                /// <summary>
                /// The <see cref="bool"/> local variable that indicates whether the input <see cref="System.Nullable{T}"/> instance has a value
                /// </summary>
                [LocalType(typeof(bool))]
                HasValue
            }

            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during deserialization
            /// </summary>
            public enum Read
            {
                /// <summary>
                /// The target <typeparamref name="T"/> array
                /// </summary>
                ArrayT,

                /// <summary>
                /// The <see cref="int"/> local variable to track the length of the target <typeparamref name="T"/> array
                /// </summary>
                [LocalType(typeof(int))]
                Length,

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
        }
    }
}


using BinaryPack.Attributes;

namespace BinaryPack.Serialization.Processors
{
    internal sealed partial class ICollectionProcessor<T>
    {
        /// <summary>
        /// A <see langword="class"/> that exposes hardcoded indices for local variables for <see cref="ICollectionProcessor{T}"/>
        /// </summary>
        private static class Locals
        {
            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during serialization
            /// </summary>
            public enum Write
            {
                /// <summary>
                /// The <see cref="System.Collections.Generic.IEnumerator{T}"/> instance used to enumerate over the input items
                /// </summary>
                IEnumeratorT
            }

            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during deserialization
            /// </summary>
            public enum Read
            {
                /// <summary>
                /// The number of items that were serialized from the original <see cref="System.Collections.Generic.ICollection{T}"/> instance
                /// </summary>
                [LocalType(typeof(int))]
                Count,


                /// <summary>
                /// The <typeparamref name="T"/> array that will contain the items being deserialized
                /// </summary>
                ArrayT,

                /// <summary>
                /// The <see cref="int"/> local variable for the loop counter
                /// </summary>
                [LocalType(typeof(int))]
                I,

                /// <summary>
                /// The <see langword="ref"/> <typeparamref name="T"/> variable used to quickly index items in the resulting <typeparamref name="T"/> array
                /// </summary>
                RefT
            }
        }
    }
}

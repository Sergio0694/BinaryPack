using BinaryPack.Attributes;

namespace BinaryPack.Serialization.Processors.Collections.Abstract
{
    internal abstract partial class ICollectionProcessorBase<TCollection, TElement>
    {
        /// <summary>
        /// A <see langword="class"/> that exposes hardcoded indices for local variables for <see cref="ICollectionProcessorBase{TCollection, TElement}"/>
        /// </summary>
        private static class Locals
        {
            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during serialization
            /// </summary>
            public enum Write
            {
                /// <summary>
                /// The <see cref="System.Collections.Generic.IEnumerator{TElement}"/> instance used to enumerate over the input items
                /// </summary>
                IEnumeratorT
            }

            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during deserialization
            /// </summary>
            public enum Read
            {
                /// <summary>
                /// The <typeparamref name="TElement"/> array that will contain the items being deserialized
                /// </summary>
                ArrayT,

                /// <summary>
                /// The number of items that were serialized from the original <typeparamref name="TCollection"/> instance
                /// </summary>
                [LocalType(typeof(int))]
                Count,

                /// <summary>
                /// The <see cref="int"/> local variable for the loop counter
                /// </summary>
                [LocalType(typeof(int))]
                I,

                /// <summary>
                /// The <see langword="ref"/> <typeparamref name="TElement"/> variable used to quickly index items in the resulting <typeparamref name="TElement"/> array
                /// </summary>
                RefT
            }
        }
    }
}

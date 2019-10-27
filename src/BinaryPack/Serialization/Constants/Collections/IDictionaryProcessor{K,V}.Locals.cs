using BinaryPack.Attributes;

namespace BinaryPack.Serialization.Processors.Collections
{
    internal sealed partial class IDictionaryProcessor<K, V>
    {
        /// <summary>
        /// A <see langword="class"/> that exposes hardcoded indices for local variables for <see cref="IDictionaryProcessor{K,V}"/>
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
                IEnumeratorT,

                /// <summary>
                /// The <see cref="System.Collections.Generic.KeyValuePair{TKey,TValue}"/> instance with the current pair of values
                /// </summary>
                KeyValuePairKV,
            }

            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during deserialization
            /// </summary>
            public enum Read
            {
                /// <summary>
                /// The target <see cref="System.Collections.Generic.Dictionary{K,V}"/> instance being deserialized
                /// </summary>
                DictionaryKV,

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

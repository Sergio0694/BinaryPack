using BinaryPack.Attributes;

namespace BinaryPack.Serialization.Processors.Collections
{
    internal sealed partial class DictionaryProcessor<K, V>
    {
        /// <summary>
        /// A <see langword="class"/> that exposes hardcoded indices for local variables for <see cref="DictionaryProcessor{K,V}"/>
        /// </summary>
        private static class Locals
        {
            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during serialization
            /// </summary>
            public enum Write
            {
                /// <summary>
                /// The <see cref="int"/> local variable to track the number of items in the source <see cref="System.Collections.Generic.Dictionary{K,V}"/> instance
                /// </summary>
                [LocalType(typeof(int))]
                Count,

                /// <summary>
                /// The <see cref="int"/> local variable for the loop counter
                /// </summary>
                [LocalType(typeof(int))]
                I,

                /// <summary>
                /// The <see langword="ref"/> <typeparamref name="K"/> variable, used to iterate arrays of reference types
                /// </summary>
                RefEntry
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

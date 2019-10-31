using System.Collections;
using BinaryPack.Attributes;

namespace BinaryPack.Serialization.Processors.Arrays
{
    internal sealed partial class BitArrayProcessor
    {
        /// <summary>
        /// A <see langword="class"/> that exposes hardcoded indices for local variables for <see cref="BitArrayProcessor"/>
        /// </summary>
        private static class Locals
        {
            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during deserialization
            /// </summary>
            public enum Read
            {
                /// <summary>
                /// The target <see cref="System.Collections.BitArray"/>
                /// </summary>
                [LocalType(typeof(BitArray))]
                BitArray,

                /// <summary>
                /// The <see cref="int"/> local variable to track the length of the target <see cref="System.Collections.BitArray"/> instance
                /// </summary>
                [LocalType(typeof(int))]
                Length
            }
        }
    }
}

namespace BinaryPack.Serialization.Processors
{
    internal sealed partial class IEnumerableProcessor<T>
    {
        /// <summary>
        /// A <see langword="class"/> that exposes hardcoded indices for local variables for <see cref="IEnumerableProcessor{T}"/>
        /// </summary>
        private static class Locals
        {
            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during serialization
            /// </summary>
            public enum Write
            {
                IEnumeratorT
            }

            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during deserialization
            /// </summary>
            public enum Read
            {
            }
        }
    }
}

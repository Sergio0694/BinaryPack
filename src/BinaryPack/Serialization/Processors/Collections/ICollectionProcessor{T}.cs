using System.Collections.Generic;
using BinaryPack.Attributes;
using BinaryPack.Serialization.Processors.Collections.Abstract;

namespace BinaryPack.Serialization.Processors.Collections
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="ICollection{T}"/> types
    /// </summary>
    /// <typeparam name="T">The type of items in <see cref="ICollection{T}"/> instances to serialize and deserialize</typeparam>
    [ProcessorId(2)]
    internal sealed class ICollectionProcessor<T> : ICollectionProcessorBase<ICollection<T>, T>
    {
        /// <summary>
        /// Gets the singleton <see cref="ICollectionProcessor{T}"/> instance to use
        /// </summary>
        public static ICollectionProcessor<T> Instance { get; } = new ICollectionProcessor<T>();
    }
}

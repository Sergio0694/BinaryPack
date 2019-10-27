using System.Collections.Generic;
using BinaryPack.Attributes;
using BinaryPack.Serialization.Processors.Collections.Abstract;

namespace BinaryPack.Serialization.Processors.Collections
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="IReadOnlyCollection{T}"/> types
    /// </summary>
    /// <typeparam name="T">The type of items in <see cref="IReadOnlyCollection{T}"/> instances to serialize and deserialize</typeparam>
    [ProcessorId(3)]
    internal sealed class IReadOnlyCollectionProcessor<T> : ICollectionProcessorBase<IReadOnlyCollection<T>, T>
    {
        /// <summary>
        /// Gets the singleton <see cref="IReadOnlyCollectionProcessor{T}"/> instance to use
        /// </summary>
        public static IReadOnlyCollectionProcessor<T> Instance { get; } = new IReadOnlyCollectionProcessor<T>();
    }
}

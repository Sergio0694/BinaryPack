using System.Collections.Generic;
using BinaryPack.Attributes;
using BinaryPack.Serialization.Processors.Collections.Abstract;

namespace BinaryPack.Serialization.Processors.Collections
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="IReadOnlyDictionary{TKey,TValue}"/> types
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary to serialize and deserialize</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary to serialize and deserialize</typeparam>
    [ProcessorId(2)]
    internal sealed class IReadOnlyDictionaryProcessor<TKey, TValue> : IDictionaryProcessorBase<IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, TKey, TValue>
    {
        /// <summary>
        /// Gets the singleton <see cref="IReadOnlyDictionaryProcessor{TKey,TValue}"/> instance to use
        /// </summary>
        public static IReadOnlyDictionaryProcessor<TKey, TValue> Instance { get; } = new IReadOnlyDictionaryProcessor<TKey, TValue>();
    }
}

using System.Collections.Generic;
using BinaryPack.Attributes;
using BinaryPack.Serialization.Processors.Collections.Abstract;

namespace BinaryPack.Serialization.Processors.Collections
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="IDictionary{TKey,TValue}"/> types
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary to serialize and deserialize</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary to serialize and deserialize</typeparam>
    [ProcessorId(1)]
    internal sealed class IDictionaryProcessor<TKey, TValue> : IDictionaryProcessorBase<IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, TKey, TValue>
    {
        /// <summary>
        /// Gets the singleton <see cref="IDictionaryProcessor{TKey,TValue}"/> instance to use
        /// </summary>
        public static IDictionaryProcessor<TKey, TValue> Instance { get; } = new IDictionaryProcessor<TKey, TValue>();
    }
}

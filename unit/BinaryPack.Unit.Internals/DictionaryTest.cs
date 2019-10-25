using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BinaryPack.Models;
using BinaryPack.Serialization.Buffers;
using BinaryPack.Serialization.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit.Internals
{
    [TestClass]
    public class DictionaryTest
    {
        // Test method for a generic dictionary
        public static void Test<K, V>(Dictionary<K, V>? dictionary) where K : IEquatable<K>, IComparable<K> where V : IEquatable<V>
        {
            // Serialization
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);
            DictionaryProcessor<K, V>.Instance.Serializer(dictionary, ref writer);
            Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.AsRef(writer.Span.GetPinnableReference()), writer.Span.Length);
            BinaryReader reader = new BinaryReader(span);
            return;
            Dictionary<K, V>? result = DictionaryProcessor<K, V>.Instance.Deserializer(ref reader);

            // Equality check
            if (dictionary == null) Assert.IsNull(result);
            else
            {
                Assert.IsNotNull(result);
                Assert.IsTrue(dictionary.Count == result!.Count);
                Assert.IsTrue(dictionary.OrderBy(p => p.Key).Zip(result.OrderBy(p => p.Key)).All(p =>
                    p.First.Key.Equals(p.Second.Key) &&
                    p.First.Value.Equals(p.Second.Value)));
            }
        }

        [TestMethod]
        public void IntAndReferenceTypeNullDictionarySerializationTest() => Test(default(Dictionary<int, MessagePackSampleModel>));

        [TestMethod]
        public void IntAndReferenceTypeEmptyDictionarySerializationTest() => Test(new Dictionary<int, MessagePackSampleModel>());

        [TestMethod]
        public void IntAndReferenceTypeDictionarySerializationTest1() => Test(new Dictionary<int, MessagePackSampleModel> { [17] = new MessagePackSampleModel { Compact = true, Schema = 127 } });

        [TestMethod]
        public void ReferenceTypeICollectionSerializationTest2() => Test((
            from i in Enumerable.Range(0, 10)
            let compact = i % 2 == 0
            let model = new MessagePackSampleModel { Compact = compact, Schema = i }
            select (Key: i, Value: model)).ToDictionary(p => p.Key, p => p.Value));
    }
}

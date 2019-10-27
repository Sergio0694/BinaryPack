using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BinaryPack.Models;
using BinaryPack.Models.Helpers;
using BinaryPack.Serialization.Buffers;
using BinaryPack.Serialization.Processors.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit.Internals
{
    [TestClass]
    public class DictionaryTest
    {
        // Test method for a generic dictionary
        public static void Test<K, V>(Dictionary<K, V?>? dictionary)
            where K : IEquatable<K>
            where V : class, IEquatable<V>
        {
            // Serialization
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);
            DictionaryProcessor<K, V?>.Instance.Serializer(dictionary, ref writer);
            Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.AsRef(writer.Span.GetPinnableReference()), writer.Span.Length);
            BinaryReader reader = new BinaryReader(span);
            Dictionary<K, V?>? result = DictionaryProcessor<K, V?>.Instance.Deserializer(ref reader);

            // Equality check
            Assert.IsTrue(StructuralComparer.IsMatch(dictionary, result));
        }

        [TestMethod]
        public void IntAndReferenceTypeNullDictionarySerializationTest() => Test(default(Dictionary<int, MessagePackSampleModel?>));

        [TestMethod]
        public void IntAndReferenceTypeEmptyDictionarySerializationTest() => Test(new Dictionary<int, MessagePackSampleModel?>());

        [TestMethod]
        public void IntAndReferenceTypeDictionarySerializationTest1() => Test(new Dictionary<int, MessagePackSampleModel?> { [17] = new MessagePackSampleModel { Compact = true, Schema = 127 } });

        [TestMethod]
        public void ReferenceTypeDictionarySerializationTest2() => Test((
            from i in Enumerable.Range(0, 10)
            let compact = i % 2 == 0
            let model = new MessagePackSampleModel { Compact = compact, Schema = i }
            select (i, model)).ToDictionary<(int Key, MessagePackSampleModel Value), int, MessagePackSampleModel?>(p => p.Key, p => p.Value));
    }
}

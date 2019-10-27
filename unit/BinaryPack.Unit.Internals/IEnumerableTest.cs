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
    public class IEnumerableTest
    {
        // Test method for a generic sequences of reference types
        public static void Test<T>(IEnumerable<T>? sequence) where T : class, IEquatable<T>
        {
            // Serialization
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);
            IEnumerableProcessor<T>.Instance.Serializer(sequence, ref writer);
            Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.AsRef(writer.Span.GetPinnableReference()), writer.Span.Length);
            BinaryReader reader = new BinaryReader(span);
            IEnumerable<T>? result = IEnumerableProcessor<T>.Instance.Deserializer(ref reader);

            // Equality check
            Assert.IsTrue(StructuralComparer.IsMatch(sequence, result));
        }

        [TestMethod]
        public void ReferenceTypeNullIEnumerableSerializationTest() => Test(default(IEnumerable<MessagePackSampleModel>));

        [TestMethod]
        public void ReferenceTypeEmptyIEnumerableSerializationTest() => Test(Array.Empty<MessagePackSampleModel>());

        [TestMethod]
        public void ReferenceTypeIEnumerableSerializationTest1() => Test(new[] { new MessagePackSampleModel { Compact = true, Schema = 17 } });

        [TestMethod]
        public void ReferenceTypeIEnumerableSerializationTest2() => Test((
            from i in Enumerable.Range(0, 10)
            let compact = i % 2 == 0
            let model = new MessagePackSampleModel { Compact = compact, Schema = i }
            select model).ToArray());

        [TestMethod]
        public void ReferenceTypeIEnumerableSerializationTest3() => Test((
            from i in Enumerable.Range(0, 10)
            let compact = i % 2 == 0
            let model = compact ? null : new MessagePackSampleModel { Compact = compact, Schema = i }
            select model).ToArray());

        [TestMethod]
        public void StringNullIEnumerableSerializationTest() => Test(default(IEnumerable<string>));

        [TestMethod]
        public void StringEmptyIEnumerableSerializationTest() => Test(Array.Empty<string>());

        [TestMethod]
        public void StringIEnumerableSerializationTest1() => Test(new[] { RandomProvider.NextString(60) });

        [TestMethod]
        public void StringIEnumerableSerializationTest2() => Test((
            from _ in Enumerable.Range(0, 10)
            select RandomProvider.NextString(60)).ToArray());

        [TestMethod]
        public void StringIEnumerableSerializationTest3() => Test((
            from i in Enumerable.Range(0, 10)
            let isNull = i % 2 == 0
            let text = isNull ? null : RandomProvider.NextString(60)
            select text).ToArray());

        // Test method for sequences of an unmanaged type
        public static void Test(IEnumerable<DateTime>? sequence)
        {
            // Serialization
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);
            IEnumerableProcessor<DateTime>.Instance.Serializer(sequence, ref writer);
            Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.AsRef(writer.Span.GetPinnableReference()), writer.Span.Length);
            BinaryReader reader = new BinaryReader(span);
            IEnumerable<DateTime>? result = IEnumerableProcessor<DateTime>.Instance.Deserializer(ref reader);

            // Equality check
            Assert.IsTrue(StructuralComparer.IsMatch(sequence, result));
        }

        [TestMethod]
        public void UnmanagedTypeNullIEnumerableSerializationTest() => Test(default);

        [TestMethod]
        public void UnmanagedTypeEmptyIEnumerableSerializationTest() => Test(Array.Empty<DateTime>());

        [TestMethod]
        public void UnmanagedTypeIEnumerableSerializationTest1() => Test(new[] { RandomProvider.NextDateTime() });

        [TestMethod]
        public void UnmanagedTypeIEnumerableSerializationTest2() => Test((
            from i in Enumerable.Range(0, 10)
            select RandomProvider.NextDateTime()).ToArray());
    }
}

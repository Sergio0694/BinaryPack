using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BinaryPack.Models;
using BinaryPack.Models.Helpers;
using BinaryPack.Serialization.Buffers;
using BinaryPack.Serialization.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit.Internals
{
    [TestClass]
    public class ICollectionTest
    {
        // Test method for a generic collection of reference types
        public static void Test<T>(ICollection<T>? sequence) where T : class, IEquatable<T>
        {
            // Serialization
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);
            ICollectionProcessor<T>.Instance.Serializer(sequence, ref writer);
            Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.AsRef(writer.Span.GetPinnableReference()), writer.Span.Length);
            BinaryReader reader = new BinaryReader(span);
            ICollection<T>? result = ICollectionProcessor<T>.Instance.Deserializer(ref reader);

            // Equality check
            Assert.IsTrue(StructuralComparer.IsMatch(sequence, result));
        }

        [TestMethod]
        public void ReferenceTypeNullICollectionSerializationTest() => Test(default(ICollection<MessagePackSampleModel>));

        [TestMethod]
        public void ReferenceTypeEmptyICollectionSerializationTest() => Test(Array.Empty<MessagePackSampleModel>());

        [TestMethod]
        public void ReferenceTypeICollectionSerializationTest1() => Test(new[] { new MessagePackSampleModel { Compact = true, Schema = 17 } });

        [TestMethod]
        public void ReferenceTypeICollectionSerializationTest2() => Test((
            from i in Enumerable.Range(0, 10)
            let compact = i % 2 == 0
            let model = new MessagePackSampleModel { Compact = compact, Schema = i }
            select model).ToArray());

        [TestMethod]
        public void ReferenceTypeICollectionSerializationTest3() => Test((
            from i in Enumerable.Range(0, 10)
            let compact = i % 2 == 0
            let model = compact ? null : new MessagePackSampleModel { Compact = compact, Schema = i }
            select model).ToArray());

        [TestMethod]
        public void StringNullICollectionSerializationTest() => Test(default(ICollection<string>));

        [TestMethod]
        public void StringEmptyICollectionSerializationTest() => Test(Array.Empty<string>());

        [TestMethod]
        public void StringICollectionSerializationTest1() => Test(new[] { RandomProvider.NextString(60) });

        [TestMethod]
        public void StringICollectionSerializationTest2() => Test((
            from _ in Enumerable.Range(0, 10)
            select RandomProvider.NextString(60)).ToArray());

        [TestMethod]
        public void StringICollectionSerializationTest3() => Test((
            from i in Enumerable.Range(0, 10)
            let isNull = i % 2 == 0
            let text = isNull ? null : RandomProvider.NextString(60)
            select text).ToArray());

        // Test method for collections of an unmanaged type
        public static void Test(ICollection<DateTime>? sequence)
        {
            // Serialization
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);
            ICollectionProcessor<DateTime>.Instance.Serializer(sequence, ref writer);
            Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.AsRef(writer.Span.GetPinnableReference()), writer.Span.Length);
            BinaryReader reader = new BinaryReader(span);
            ICollection<DateTime>? result = ICollectionProcessor<DateTime>.Instance.Deserializer(ref reader);

            // Equality check
            Assert.IsTrue(StructuralComparer.IsMatch(sequence, result));
        }

        [TestMethod]
        public void UnmanagedTypeNullICollectionSerializationTest() => Test(default);

        [TestMethod]
        public void UnmanagedTypeEmptyICollectionSerializationTest() => Test(Array.Empty<DateTime>());

        [TestMethod]
        public void UnmanagedTypeICollectionSerializationTest1() => Test(new[] { RandomProvider.NextDateTime() });

        [TestMethod]
        public void UnmanagedTypeICollectionSerializationTest2() => Test((
            from i in Enumerable.Range(0, 10)
            select RandomProvider.NextDateTime()).ToArray());
    }
}

using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BinaryPack.Models.Helpers;
using BinaryPack.Serialization.Buffers;
using BinaryPack.Serialization.Processors.Arrays;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit.Internals
{
    [TestClass]
    public class BitArrayTest
    {
        // Test method for a generic BitArray instance
        public static void Test(BitArray? array)
        {
            // Serialization
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);
            BitArrayProcessor.Instance.Serializer(array, ref writer);
            Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.AsRef(writer.Span.GetPinnableReference()), writer.Span.Length);
            BinaryReader reader = new BinaryReader(span);
            BitArray? result = BitArrayProcessor.Instance.Deserializer(ref reader);

            // Equality check
            if (array == null) Assert.IsNull(result);
            else
            {
                Assert.IsNotNull(result);
                Assert.IsTrue(array.Length == result!.Length);
                Assert.IsTrue(StructuralComparer.IsMatch(array.Cast<bool>(), result.Cast<bool>()));
            }
        }

        [TestMethod]
        public void NullBitArraySerializationTest() => Test(null);

        [TestMethod]
        public void EmptyBitArraySerializationTest() => Test(new BitArray(0));

        [TestMethod]
        public void BitArrayTest1() => Test(new BitArray(Enumerable.Range(0, 128).Select(_ => RandomProvider.NextBool()).ToArray()));

        [TestMethod]
        public void BitArrayTest2() => Test(new BitArray(Enumerable.Range(0, 377).Select(_ => RandomProvider.NextBool()).ToArray()));
    }
}

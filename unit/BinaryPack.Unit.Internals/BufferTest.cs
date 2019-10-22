using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BinaryPack.Models.Helpers;
using BinaryPack.Serialization.Buffers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit.Internals
{
    [TestClass]
    public class BufferTest
    {
        [TestMethod]
        public void WriteSingleValues()
        {
            // Write
            BinaryWriter writer = new BinaryWriter(2);
            writer.Write((byte)127);
            writer.Write(int.MaxValue);
            writer.Write(3.14f);
            writer.Write(6.28);
            writer.Write(DateTime.MaxValue);

            // Verify
            ref byte r0 = ref MemoryMarshal.GetReference(writer.Span);
            Assert.IsTrue(Unsafe.As<byte, byte>(ref Unsafe.Add(ref r0, 0)) == 127);
            Assert.IsTrue(Unsafe.As<byte, int>(ref Unsafe.Add(ref r0, 1)) == int.MaxValue);
            Assert.IsTrue(MathF.Abs(Unsafe.As<byte, float>(ref Unsafe.Add(ref r0, 5)) - 3.14f) < 0.001f);
            Assert.IsTrue(Math.Abs(Unsafe.As<byte, double>(ref Unsafe.Add(ref r0, 9)) - 6.28) < 0.001f);
            Assert.IsTrue(Unsafe.As<byte, DateTime>(ref Unsafe.Add(ref r0, 17)) == DateTime.MaxValue);
        }

        [TestMethod]
        public void WriteSpan()
        {
            // Populate random data
            Span<byte> span = stackalloc byte[1024];
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = (byte)(RandomProvider.NextInt() % 255);
            }

            // Write
            BinaryWriter writer = new BinaryWriter(2);
            writer.Write(span);

            // Verify
            Assert.IsTrue(span.SequenceEqual(writer.Span));
        }

        [TestMethod]
        public void ReadSingleValues()
        {
            // Write
            byte[] array = new byte[128];
            ref byte r0 = ref array[0];
            Unsafe.As<byte, int>(ref Unsafe.Add(ref r0, 0)) = 127;
            Unsafe.As<byte, float>(ref Unsafe.Add(ref r0, 4)) = 3.14f;
            Unsafe.As<byte, double>(ref Unsafe.Add(ref r0, 8)) = 6.28;
            Unsafe.As<byte, DateTime>(ref Unsafe.Add(ref r0, 16)) = DateTime.MaxValue;

            // Verify
            BinaryReader reader = new BinaryReader(array);
            Assert.IsTrue(reader.Read<int>() == 127);
            Assert.IsTrue(MathF.Abs(reader.Read<float>() - 3.14f) < 0.001f);
            Assert.IsTrue(Math.Abs(reader.Read<double>() - 6.28) < 0.001f);
            Assert.IsTrue(reader.Read<DateTime>() == DateTime.MaxValue);
        }

        [TestMethod]
        public void ReadSpan()
        {
            // Populate random data
            byte[] array = new byte[1024];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (byte)(RandomProvider.NextInt() % 255);
            }

            // Read
            BinaryReader reader = new BinaryReader(array);
            Span<byte> target = new byte[array.Length];
            reader.Read(target);

            // Verify
            Assert.IsTrue(array.AsSpan().SequenceEqual(target));
        }
    }
}

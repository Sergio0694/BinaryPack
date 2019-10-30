using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BinaryPack.Models;
using BinaryPack.Models.Helpers;
using BinaryPack.Serialization.Buffers;
using BinaryPack.Serialization.Processors.Arrays;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit.Internals
{
    [TestClass]
    public class ArrayTest
    {
        // Test method for a generic ND arrays of reference types
        public static void Test<T>(T?[,]? array) where T : class, IEquatable<T>
        {
            // Serialization
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);
            ArrayProcessor<T?[,]>.Instance.Serializer(array, ref writer);
            Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.AsRef(writer.Span.GetPinnableReference()), writer.Span.Length);
            BinaryReader reader = new BinaryReader(span);
            T?[,]? result = ArrayProcessor<T?[,]>.Instance.Deserializer(ref reader);

            // Equality check
            Assert.IsTrue(StructuralComparer.IsMatch(array?.Cast<T>(), result?.Cast<T>()));
        }

        [TestMethod]
        public void ReferenceTypeNullArraySerializationTest() => Test(default(MessagePackSampleModel[,]));

        [TestMethod]
        public void ReferenceTypeEmptyArraySerializationTest() => Test(new MessagePackSampleModel[0, 0]);

        [TestMethod]
        public void ReferenceTypeArraySerializationTest1() => Test(new[,] { { new MessagePackSampleModel { Compact = true, Schema = 17 }, new MessagePackSampleModel { Compact = true, Schema = 33, } } });

        [TestMethod]
        public void ReferenceTypeArraySerializationTest2()
        {
            MessagePackSampleModel[,] array = new MessagePackSampleModel[5, 2];
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    bool compact = i % 2 == 0;
                    array[i, j] = new MessagePackSampleModel { Compact = compact, Schema = i };

                }
            }

            Test(array);
        }

        [TestMethod]
        public void ReferenceTypeArraySerializationTest3()
        {
            MessagePackSampleModel?[,] array = new MessagePackSampleModel?[5, 2];
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    bool compact = i % 2 == 0;
                    array[i, j] = compact ? null : new MessagePackSampleModel { Compact = true, Schema = i };

                }
            }

            Test(array);
        }

        [TestMethod]
        public void StringNullArraySerializationTest() => Test(default(string[,]));

        [TestMethod]
        public void StringEmptyArraySerializationTest() => Test(new string[2, 0]);

        [TestMethod]
        public void StringArraySerializationTest1() => Test(new[,] { { "Hello world!", "Hi Bob!" } });

        [TestMethod]
        public void StringArraySerializationTest2()
        {
            string?[,] array = new string?[5, 2];
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    array[i, j] = RandomProvider.NextString(60);

                }
            }

            Test(array);
        }

        [TestMethod]
        public void StringArraySerializationTest3()
        {
            string?[,] array = new string?[5, 2];
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    bool isNull = i % 2 == 0;
                    array[i, j] = isNull ? null : RandomProvider.NextString(60);

                }
            }

            Test(array);
        }

        // Test method for arrays of an unmanaged type
        public static void Test(DateTime[,]? array)
        {
            // Serialization
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);
            ArrayProcessor<DateTime[,]>.Instance.Serializer(array, ref writer);
            Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.AsRef(writer.Span.GetPinnableReference()), writer.Span.Length);
            BinaryReader reader = new BinaryReader(span);
            DateTime[,]? result = ArrayProcessor<DateTime[,]>.Instance.Deserializer(ref reader);

            // Equality check
            Assert.IsTrue(StructuralComparer.IsMatch(array?.Cast<DateTime>(), result?.Cast<DateTime>()));
        }

        [TestMethod]
        public void UnmanagedTypeNullArraySerializationTest() => Test(default);

        [TestMethod]
        public void UnmanagedTypeEmptyArraySerializationTest() => Test(new DateTime[0, 1]);

        [TestMethod]
        public void UnmanagedTypeArraySerializationTest1() => Test(new[,] { { RandomProvider.NextDateTime(), RandomProvider.NextDateTime() } });

        [TestMethod]
        public void UnmanagedTypeArraySerializationTest2()
        {
            DateTime[,] array = new DateTime[5, 2];
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    array[i, j] = RandomProvider.NextDateTime();

                }
            }

            Test(array);
        }
    }
}

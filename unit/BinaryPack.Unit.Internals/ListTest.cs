using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BinaryPack.Models;
using BinaryPack.Models.Helpers;
using BinaryPack.Serialization.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit.Internals
{
    [TestClass]
    public class ListTest
    {
        // Test method for a generic list of reference types
        public static void Test<T>(List<T>? list) where T : class, IEquatable<T>
        {
            // Serialization
            using MemoryStream stream = new MemoryStream();
            ListProcessor<T>.Instance.Serializer(list, stream);
            stream.Seek(0, SeekOrigin.Begin);
            List<T>? result = ListProcessor<T>.Instance.Deserializer(stream);

            // Equality check
            if (list == null) Assert.IsNull(result);
            else
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(list.Count, result!.Count);
                Assert.IsTrue(list.Zip(result).All(p =>
                {
                    if (p.First == null && p.Second == null) return true;
                    return p.First?.Equals(p.Second) == true;
                }));
            }
        }

        [TestMethod]
        public void ReferenceTypeNullListSerializationTest() => Test(default(List<MessagePackSampleModel>));

        [TestMethod]
        public void ReferenceTypeEmptyListSerializationTest() => Test(new List<MessagePackSampleModel>());

        [TestMethod]
        public void ReferenceTypeListSerializationTest1() => Test(new List<MessagePackSampleModel> { new MessagePackSampleModel { Compact = true, Schema = 17 } });

        [TestMethod]
        public void ReferenceTypeListSerializationTest2() => Test((
            from i in Enumerable.Range(0, 10)
            let compact = i % 2 == 0
            let model = new MessagePackSampleModel { Compact = compact, Schema = i }
            select model).ToList());

        [TestMethod]
        public void ReferenceTypeListSerializationTest3() => Test((
            from i in Enumerable.Range(0, 10)
            let compact = i % 2 == 0
            let model = compact ? null : new MessagePackSampleModel { Compact = compact, Schema = i }
            select model).ToList());

        [TestMethod]
        public void StringNullListSerializationTest() => Test(default(List<string>));

        [TestMethod]
        public void StringEmptyListSerializationTest() => Test(new List<string>());

        [TestMethod]
        public void StringListSerializationTest1() => Test(new List<string> { RandomProvider.NextString(60) });

        [TestMethod]
        public void StringListSerializationTest2() => Test((
            from _ in Enumerable.Range(0, 10)
            select RandomProvider.NextString(60)).ToList());

        [TestMethod]
        public void StringListSerializationTest3() => Test((
            from i in Enumerable.Range(0, 10)
            let isNull = i % 2 == 0
            let text = isNull ? null : RandomProvider.NextString(60)
            select text).ToList());

        // Test method for list of an unmanaged type
        public static void Test(List<DateTime>? list)
        {
            // Serialization
            using MemoryStream stream = new MemoryStream();
            ListProcessor<DateTime>.Instance.Serializer(list, stream);
            stream.Seek(0, SeekOrigin.Begin);
            List<DateTime>? result = ListProcessor<DateTime>.Instance.Deserializer(stream);

            // Equality check
            if (list == null) Assert.IsNull(result);
            else
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(list.Count, result!.Count);
                Assert.IsTrue(MemoryMarshal.AsBytes(list.ToArray().AsSpan()).SequenceEqual(MemoryMarshal.AsBytes(result.ToArray().AsSpan())));
            }
        }

        [TestMethod]
        public void UnmanagedTypeNullListSerializationTest() => Test(default);

        [TestMethod]
        public void UnmanagedTypeEmptyListSerializationTest() => Test(new List<DateTime>());

        [TestMethod]
        public void UnmanagedTypeListSerializationTest1() => Test(new List<DateTime> { RandomProvider.NextDateTime() });

        [TestMethod]
        public void UnmanagedTypeListSerializationTest2() => Test((
            from i in Enumerable.Range(0, 10)
            select RandomProvider.NextDateTime()).ToList());
    }
}

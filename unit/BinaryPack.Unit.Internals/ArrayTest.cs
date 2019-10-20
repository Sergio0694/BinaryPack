using System;
using System.IO;
using System.Linq;
using BinaryPack.Models;
using BinaryPack.Models.Interfaces;
using BinaryPack.Serialization.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit.Internals
{
    [TestClass]
    public class ArrayTest
    {
        [TestMethod]
        public void ReferenceTypesNullArraySerializationTest() => Test(default(MessagePackSampleModel[]));

        [TestMethod]
        public void ReferenceTypesEmptyArraySerializationTest() => Test(Array.Empty<MessagePackSampleModel>());

        [TestMethod]
        public void ReferenceTypesArraySerializationTest1() => Test(new[] { new MessagePackSampleModel { Compact = true, Schema = 17 } });

        [TestMethod]
        public void ReferenceTypesArraySerializationTest2() => Test((
            from i in Enumerable.Range(0, 10)
            let compact = i % 2 == 0
            let model = new MessagePackSampleModel {Compact = compact, Schema = i}
            select model).ToArray());

        [TestMethod]
        public void ReferenceTypesArraySerializationTest3() => Test((
            from i in Enumerable.Range(0, 10)
            let compact = i % 2 == 0
            let model = compact ? null : new MessagePackSampleModel { Compact = compact, Schema = i }
            select model).ToArray());

        // Test method for a generic arrays
        public static void Test<T>(T[]? array) where T : class, IInitializable, IEquatable<T>, new()
        {
            // Serialization
            using MemoryStream stream = new MemoryStream();
            ArrayProcessor<T>.Instance.Serializer(array, stream);
            stream.Seek(0, SeekOrigin.Begin);
            var result = ArrayProcessor<T>.Instance.Deserializer(stream);

            // Equality check
            if (array == null) Assert.IsNull(result);
            else
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(array.Length, result!.Length);
                Assert.IsTrue(array.Zip(result).All(p =>
                {
                    if (p.First == null && p.Second == null) return true;
                    return p.First?.Equals(p.Second) == true;
                }));
            }
        }
    }
}
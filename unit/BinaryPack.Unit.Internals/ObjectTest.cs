using System;
using System.IO;
using BinaryPack.Models;
using BinaryPack.Models.Interfaces;
using BinaryPack.Serialization.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BinaryWriter = BinaryPack.Serialization.Buffers.BinaryWriter;

namespace BinaryPack.Unit.Internals
{
    [TestClass]
    public class ObjectTest
    {
        // Test method for reference types
        private static void Test<T>() where T : class, IInitializable, IEquatable<T>, new()
        {
            // Initialization
            T obj = new T();
            obj.Initialize();

            // Serialization
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);
            ObjectProcessor<T>.Instance.Serializer(obj, ref writer);
            using Stream stream = new MemoryStream(writer.Span.ToArray());
            T result = ObjectProcessor<T>.Instance.Deserializer(stream);

            // Equality check
            Assert.IsNotNull(result);
            Assert.IsTrue(obj.Equals(result));
        }

        [TestMethod]
        public void HelloWorld() => Test<HelloWorldModel>();

        [TestMethod]
        public void JsonResponse() => Test<JsonResponseModel>();

        [TestMethod]
        public void ListContainer() => Test<ListContainerModel>();

        [TestMethod]
        public void MessagePackSample() => Test<MessagePackSampleModel>();

        [TestMethod]
        public void NestedHierarchySimple() => Test<NestedHierarchySimpleModel>();

        [TestMethod]
        public void UnmanagedArray() => Test<UnmanagedArrayModel>();

    }
}

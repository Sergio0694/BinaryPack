using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BinaryPack.Models;
using BinaryPack.Models.Interfaces;
using BinaryPack.Serialization.Buffers;
using BinaryPack.Serialization.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.AsRef(writer.Span.GetPinnableReference()), writer.Span.Length);
            BinaryReader reader = new BinaryReader(span);
            T result = ObjectProcessor<T>.Instance.Deserializer(ref reader);

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
        public void NeuralNetworkLayer() => Test<NeuralNetworkLayerModel>();

        [TestMethod]
        public void UnmanagedArray() => Test<UnmanagedArrayModel>();

        [TestMethod]
        public void ValidationReferenceType() => Test<ValidationReferenceTypeModel>();

    }
}

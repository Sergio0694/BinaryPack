using System.IO;
using BinaryPack.Models;
using BinaryPack.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit.Internals
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void ReferenceTypesArraySerializationTest()
        {
            var array = new[] { new MessagePackSampleModel() };
            array[0].Initialize();

            // Serialize
            using MemoryStream stream = new MemoryStream();
            ArrayProcessor<MessagePackSampleModel>.Serializer(array, stream);

            // Deserialize
            stream.Seek(0, SeekOrigin.Begin);
            var result = ArrayProcessor<MessagePackSampleModel>.Deserializer(stream);

            Assert.IsNotNull(result);
            Assert.IsTrue(array.Length == result.Length);
            Assert.IsTrue(array[0].Equals(result[0]));
        }
    }
}
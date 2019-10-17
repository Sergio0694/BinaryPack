using System.IO;
using BinaryPack.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            MessagePackSampleModel model = new MessagePackSampleModel();
            using MemoryStream stream = new MemoryStream();
            BinaryConverter.Serialize(model, stream);
        }
    }
}

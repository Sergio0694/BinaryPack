using BinaryPack.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void MessagePackSample() => TestRunner.Test<MessagePackSampleModel>();
    }
}

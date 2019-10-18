using BinaryPack.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void MessagePackSample() => TestRunner.Test<MessagePackSampleModel>();

        [TestMethod]
        public void HelloWorldSample1() => TestRunner.Test<HelloWorldModel>();

        [TestMethod]
        public void HelloWorldSample2()
        {
            HelloWorldModel model = new HelloWorldModel { Value = 17 };
            TestRunner.Test(model);
        }

        [TestMethod]
        public void HelloWorldSample3()
        {
            HelloWorldModel model = new HelloWorldModel { Property = "", Value = 999 };
            TestRunner.Test(model);
        }
    }
}

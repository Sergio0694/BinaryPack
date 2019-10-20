using System;
using BinaryPack.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void MessagePackNullSample() => TestRunner.TestNull<MessagePackSampleModel>();

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

        [TestMethod]
        public void UnmanagedArraySample1() => TestRunner.Test<UnmanagedArrayModel>();

        [TestMethod]
        public void UnmanagedArraySample2()
        {
            UnmanagedArrayModel model = new UnmanagedArrayModel { Value = 17 };
            TestRunner.Test(model);
        }

        [TestMethod]
        public void UnmanagedArraySample3()
        {
            UnmanagedArrayModel model = new UnmanagedArrayModel { Items = Array.Empty<DateTime>(), Value = 999 };
            TestRunner.Test(model);
        }

        [TestMethod]
        public void NestedHierarchyNullSample() => TestRunner.Test(new NestedHierarchySimpleModel());

        [TestMethod]
        public void NestedHierarchySample1() => TestRunner.Test<NestedHierarchySimpleModel>();

        [TestMethod]
        public void JsonResponseSample() => TestRunner.Test<JsonResponseModel>();
    }
}

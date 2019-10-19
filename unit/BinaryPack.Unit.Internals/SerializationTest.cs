using System;
using System.Linq;
using BinaryPack.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit.Internals
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void ReferenceTypesNullArraySerializationTest() => TestRunner.Test(default(MessagePackSampleModel[]));

        [TestMethod]
        public void ReferenceTypesEmptyArraySerializationTest() => TestRunner.Test(Array.Empty<MessagePackSampleModel>());

        [TestMethod]
        public void ReferenceTypesArraySerializationTest1() => TestRunner.Test(new[] { new MessagePackSampleModel { Compact = true, Schema = 17 } });

        [TestMethod]
        public void ReferenceTypesArraySerializationTest2() => TestRunner.Test((
            from i in Enumerable.Range(0, 10)
            let compact = i % 2 == 0
            let model = new MessagePackSampleModel {Compact = compact, Schema = i}
            select model).ToArray());

        [TestMethod]
        public void ReferenceTypesArraySerializationTest3() => TestRunner.Test((
            from i in Enumerable.Range(0, 10)
            let compact = i % 2 == 0
            let model = compact ? null : new MessagePackSampleModel { Compact = compact, Schema = i }
            select model).ToArray());
    }
}
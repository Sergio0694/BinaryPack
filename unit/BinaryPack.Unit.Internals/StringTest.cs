using System.IO;
using BinaryPack.Serialization.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit.Internals
{
    [TestClass]
    public class StringTest
    {
        [TestMethod]
        public void NullString() => Test(null);

        [TestMethod]
        public void EmptyString() => Test(string.Empty);

        [TestMethod]
        public void ShortString() => Test("Hello world");

        [TestMethod]
        public void LongString() => Test("P!pl<C'a /2-2!N2r}N-N'[\\Ew'aoo.=grDr3oHG\")>;eZ <u yqGeyID2GCC=p/!sE>[Z'#S'+Fg?wivbiot:u!wxM H&#c7/:o5a_: v=?XSb#8[JaR 9e{CEb-'YN#F/V&(R6!Nn{{TGD7JfjXA06tTrq:}-!;m<2E*}1*4_#1;hGz!Ib7osa6vaN4ay\"Bm_.84'-LTaEa,&WlJt8RIiKwYzLHMzG8[aBYX.g\"<5a.N *q)bhjbNv$34[7Pd'W8-$Jb{2<.664(=IYpX>j2[T-h=GDONOV6(sBy]0+OKZJ8c{tj\"FuD3FZUuaCTlk");

        // Test method for a generic string
        private static void Test(string? text)
        {
            // Serialization
            using MemoryStream stream = new MemoryStream();
            StringProcessor.Instance.Serializer(text, stream);
            stream.Seek(0, SeekOrigin.Begin);
            string? result = StringProcessor.Instance.Deserializer(stream);

            // Equality check
            if (text == null) Assert.IsNull(result);
            else Assert.IsTrue(text.Equals(result));
        }
    }
}

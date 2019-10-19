using System;
using System.IO;
using System.Linq;
using BinaryPack.Models.Interfaces;
using BinaryPack.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit.Internals
{
    /// <summary>
    /// A test <see langword="class"/> with helper methods to easily test the APIs with different models
    /// </summary>
    internal static class TestRunner
    {
        /// <summary>
        /// Runs a test with a new instance of a specified type
        /// </summary>
        /// <typeparam name="T">The type of model to test</typeparam>
        public static void Test<T>(T[]? array) where T : class, IInitializable, IEquatable<T>, new()
        {
            // Serialize
            using MemoryStream stream = new MemoryStream();
            ArrayProcessor<T>.Serializer(array, stream);

            // Deserialize
            stream.Seek(0, SeekOrigin.Begin);
            var result = ArrayProcessor<T>.Deserializer(stream);

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

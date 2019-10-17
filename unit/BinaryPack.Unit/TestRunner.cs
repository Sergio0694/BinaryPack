using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit
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
        public static void Test<T>() where T : IEquatable<T>, new() => Test(new T());

        /// <summary>
        /// Runs a test with a given instance of a specified type
        /// </summary>
        /// <typeparam name="T">The type of model to test</typeparam>
        /// <param name="obj">The input model to serialize and test</param>
        public static void Test<T>(T obj) where T : IEquatable<T>, new()
        {
            // Serialize
            using MemoryStream stream = new MemoryStream();
            BinaryConverter.Serialize(obj, stream);

            // Deserialize
            stream.Seek(0, SeekOrigin.Begin);
            T result = BinaryConverter.Deserialize<T>(stream);

            Assert.IsTrue(obj.Equals(result));
        }
    }
}

using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Serialization;
using BinaryPack.Models.Interfaces;
using Newtonsoft.Json;

namespace BinaryPack.Comparison.Implementation
{
    /// <summary>
    /// A file size comparer for a generic type using different serialization libraries
    /// </summary>
    /// <typeparam name="T">The type of model to serialize</typeparam>
    internal static class FileSizeComparer
    {
        public static void Run<T>() where T : class, IInitializable, new()
        {
            T model = new T();
            model.Initialize();

            // Newtonsoft.Json serialization
            using MemoryStream jsonStream = new MemoryStream();
            using StreamWriter textWriter = new StreamWriter(jsonStream);
            using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
            new JsonSerializer().Serialize(jsonWriter, model);
            jsonWriter.Flush();

            // XML serialization
            using Stream xmlStream = new MemoryStream();
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(xmlStream, model);

            // BinaryPack serialization
            using MemoryStream binaryStream = new MemoryStream();
            BinaryConverter.Serialize(model, binaryStream);

            Console.WriteLine($">> Newtonsoft.Json:\t{jsonStream.Position} bytes");
            Console.WriteLine($">> XML serializer:\t{xmlStream.Position} bytes");
            Console.WriteLine($">> BinaryPack:\t\t{binaryStream.Position} bytes");
            Console.WriteLine($"{Environment.NewLine}Compressing with GZip...{Environment.NewLine}");

            // Newtonsoft.Json compression
            using (MemoryStream output = new MemoryStream())
            {
                using GZipStream gzip = new GZipStream(output, CompressionLevel.Optimal);

                jsonStream.Seek(0, SeekOrigin.Begin);
                jsonStream.CopyTo(gzip);

                Console.WriteLine($">> Newtonsoft.Json:\t{output.Position} bytes");
            }

            // XML serializer compression
            using (MemoryStream output = new MemoryStream())
            {
                using GZipStream gzip = new GZipStream(output, CompressionLevel.Optimal);

                xmlStream.Seek(0, SeekOrigin.Begin);
                xmlStream.CopyTo(gzip);

                Console.WriteLine($">> XML serializer:\t{output.Position} bytes");
            }

            // BinaryPack compression
            using (MemoryStream output = new MemoryStream())
            {
                using GZipStream gzip = new GZipStream(output, CompressionLevel.Optimal);

                binaryStream.Seek(0, SeekOrigin.Begin);
                binaryStream.CopyTo(gzip);

                Console.WriteLine($">> BinaryPack:\t\t{output.Position} bytes");
            }
        }
    }
}

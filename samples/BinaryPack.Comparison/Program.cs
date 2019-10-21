using System;
using System.IO;
using System.IO.Compression;
using BinaryPack.Models;
using Newtonsoft.Json;

namespace BinaryPack.Comparison
{
    class Program
    {
        static void Main()
        {
            /* ==========================
             * Binary file comparison
             * ==========================
             * This program will create a new model and populate it, then serialize
             * it both in JSON format and with BinaryPack. It will then print
             * the size in bytes of the resulting serialized data, both before
             * and after a compression with gzip. */
            JsonResponseModel model = new JsonResponseModel();
            model.Initialize();

            // Newtonsoft.Json serialization
            using MemoryStream jsonStream = new MemoryStream();
            using StreamWriter textWriter = new StreamWriter(jsonStream);
            using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
            new JsonSerializer().Serialize(jsonWriter, model);
            jsonWriter.Flush();

            // BinaryPack serialization
            using MemoryStream binaryStream = new MemoryStream();
            BinaryConverter.Serialize(model, binaryStream);

            Console.WriteLine($">> Newtonsoft.Json:\t{jsonStream.Position} bytes");
            Console.WriteLine($">> BinaryPack:\t\t{binaryStream.Position} bytes");
            Console.WriteLine("Compressing with GZip...");

            // Newtonsoft.Json compression
            using (MemoryStream output = new MemoryStream())
            {
                using GZipStream gzip = new GZipStream(output, CompressionLevel.Optimal);

                jsonStream.Seek(0, SeekOrigin.Begin);
                jsonStream.CopyTo(gzip);

                Console.WriteLine($">> Newtonsoft.Json:\t{output.Position} bytes");
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

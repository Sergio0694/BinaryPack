using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using BinaryPack.Models.Interfaces;
using MessagePack;
using MessagePack.Resolvers;
using Portable.Xaml;
using Utf8Json;

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

            // JSON
            var json = CalculateFileSize(stream => JsonSerializer.Serialize(stream, model));

            // XML
            var xml = CalculateFileSize(stream =>
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stream, model);
            });

            // XAML
            var xaml = CalculateFileSize(stream => XamlServices.Save(stream, model));

            // BinaryFormatter
            var binaryFormatter = CalculateFileSize(stream =>
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, model);
            });

            // MessagePack
            var messagePack = CalculateFileSize(stream => MessagePackSerializer.Serialize(stream, model));

            // BinaryPack
            var binaryPack = CalculateFileSize(stream => BinaryConverter.Serialize(model, stream));

            // Report
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("=================");
            builder.AppendLine("Default output");
            builder.AppendLine("=================");
            builder.AppendLine($"JSON:\t\t{json.Plain}");
            builder.AppendLine($"XML:\t\t{xml.Plain}");
            builder.AppendLine($"XAML:\t\t{xaml.Plain}");
            builder.AppendLine($"BinaryForm.:\t{binaryFormatter.Plain}");
            builder.AppendLine($"MessagePack:\t{messagePack.Plain}");
            builder.AppendLine($"BinaryPack:\t{binaryPack.Plain}");
            builder.AppendLine();
            builder.AppendLine("=================");
            builder.AppendLine("GZip output");
            builder.AppendLine("=================");
            builder.AppendLine($"JSON:\t\t{json.GZip}");
            builder.AppendLine($"XML:\t\t{xml.GZip}");
            builder.AppendLine($"XAML:\t\t{xaml.GZip}");
            builder.AppendLine($"BinaryForm.:\t{binaryFormatter.GZip}");
            builder.AppendLine($"MessagePack:\t{messagePack.GZip}");
            builder.AppendLine($"BinaryPack:\t{binaryPack.GZip}");
            Console.WriteLine(builder);
        }

        /// <summary>
        /// Calculates the file size for a given serializer
        /// </summary>
        /// <param name="f">An <see cref="Action{T}"/> that writes the serialized data to a given <see cref="Stream"/></param>
        [Pure]
        private static (long Plain, long GZip) CalculateFileSize(Action<Stream> f)
        {
            using MemoryStream stream = new MemoryStream();
            f(stream);

            long plain = stream.Position;

            using MemoryStream output = new MemoryStream();
            using GZipStream gzip = new GZipStream(output, CompressionLevel.Optimal);

            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(gzip);

            return (plain, output.Position);
        }
    }
}

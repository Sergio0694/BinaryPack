using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BinaryPack.Models.Interfaces;
using JsonTextWriter = Newtonsoft.Json.JsonTextWriter;
using JsonTextReader = Newtonsoft.Json.JsonTextReader;
using Utf8JsonWriter = System.Text.Json.Utf8JsonWriter;
using Utf8JsonSerializer = Utf8Json.JsonSerializer;

namespace BinaryPack.Benchmark.Implementations
{
    /// <summary>
    /// A benchmark for a generic type using different serialization libraries
    /// </summary>
    /// <typeparam name="T">The type of model to serialize</typeparam>
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public partial class Benchmark<T> where T : class, IInitializable, new()
    {
        private const string SERIALIZATION = "Serialization";

        private const string DESERIALIZATION = "Deserialization";

        // Number of iterations to run
        private const int N = 1000;

        private readonly T Model = new T();

        private readonly Stream NewtonsoftStream = new MemoryStream();

        private readonly Stream BinaryFormatterStream = new MemoryStream();

        private readonly Stream DotNetCoreJsonStream = new MemoryStream();

        private readonly Stream DataContractJsonStream = new MemoryStream();

        private readonly Stream XmlSerializerStream = new MemoryStream();

        private readonly Stream PortableXamlStream = new MemoryStream();

        private readonly Stream Utf8JsonStream = new MemoryStream();

        private readonly Stream BinaryPackStream = new MemoryStream();

        /// <summary>
        /// Initial setup for a benchmarking session
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            Model.Initialize();

            // Newtonsoft
            {
                using StreamWriter textWriter = new StreamWriter(NewtonsoftStream);
                using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);

                var serializer = new Newtonsoft.Json.JsonSerializer();
                serializer.Serialize(jsonWriter, Model);
                jsonWriter.Flush();

                NewtonsoftStream.Seek(0, SeekOrigin.Begin);
                using StreamReader textReader = new StreamReader(NewtonsoftStream);
                using JsonTextReader jsonReader = new JsonTextReader(textReader);
                _ = serializer.Deserialize<T>(jsonReader);
            }

            // Binary formatter
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(BinaryFormatterStream, Model);

                BinaryFormatterStream.Seek(0, SeekOrigin.Begin);
                _ = formatter.Deserialize(BinaryFormatterStream);
            }

            // .NETCore JSON
            {
                using Utf8JsonWriter jsonWriter = new Utf8JsonWriter(DotNetCoreJsonStream);

                System.Text.Json.JsonSerializer.Serialize(jsonWriter, Model);

                DotNetCoreJsonStream.Seek(0, SeekOrigin.Begin);
                _ = System.Text.Json.JsonSerializer.DeserializeAsync<T>(DotNetCoreJsonStream).Result;
            }

            // DataContractJson
            {
                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(DataContractJsonStream, Model);

                DataContractJsonStream.Seek(0, SeekOrigin.Begin);
                _ = serializer.ReadObject(DataContractJsonStream);
            }

            // XML serializer
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                serializer.Serialize(XmlSerializerStream, Model);

                XmlSerializerStream.Seek(0, SeekOrigin.Begin);
                _ = serializer.Deserialize(XmlSerializerStream);
            }

            // Portable Xaml
            {
                Portable.Xaml.XamlServices.Save(PortableXamlStream, Model);

                PortableXamlStream.Seek(0, SeekOrigin.Begin);
                _ = Portable.Xaml.XamlServices.Load(PortableXamlStream);
            }

            // Utf8Json
            {
                Utf8JsonSerializer.Serialize(Utf8JsonStream, Model);

                Utf8JsonStream.Seek(0, SeekOrigin.Begin);
                _ = Utf8JsonSerializer.Deserialize<T>(Utf8JsonStream);
            }

            // BinaryPack
            {
                BinaryConverter.Serialize(Model, BinaryPackStream);

                BinaryPackStream.Seek(0, SeekOrigin.Begin);
                _ = BinaryConverter.Deserialize<T>(BinaryPackStream);
            }
        }
    }
}

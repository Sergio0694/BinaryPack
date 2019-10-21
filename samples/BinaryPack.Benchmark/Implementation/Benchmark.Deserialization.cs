using System.IO;
using BenchmarkDotNet.Attributes;
using JsonTextReader = Newtonsoft.Json.JsonTextReader;
using Utf8JsonSerializer = Utf8Json.JsonSerializer;

namespace BinaryPack.Benchmark.Implementations
{
    public partial class Benchmark<T>
    {
        /// <summary>
        /// Deserialization powered by <see cref="Newtonsoft.Json.JsonSerializer"/>
        /// </summary>
        [Benchmark(Baseline = true)]
        [BenchmarkCategory(DESERIALIZATION)]
        public void NewtonsoftJson2()
        {
            using Stream stream = new MemoryStream(NewtonsoftJsonData);
            using StreamReader textReader = new StreamReader(stream);
            using JsonTextReader jsonReader = new JsonTextReader(textReader);
            var serializer = new Newtonsoft.Json.JsonSerializer();

            _ = serializer.Deserialize<T>(jsonReader);
        }

        /// <summary>
        /// Deserialization powered by <see cref="System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(DESERIALIZATION)]
        public void BinaryFormatter2()
        {
            using Stream stream = new MemoryStream(BinaryFormatterData);

            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            _ = formatter.Deserialize(stream);
        }

        /// <summary>
        /// Deserialization powered by <see cref="System.Text.Json.JsonSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(DESERIALIZATION)]
        public void NetCoreJson2()
        {
            using Stream stream = new MemoryStream(DotNetCoreJsonData);

            _ = System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream).Result;
        }

        /// <summary>
        /// Deserialization powered by <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(DESERIALIZATION)]
        public void DataContractJsonSerializer2()
        {
            using Stream stream = new MemoryStream(DataContractJsonData);

            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
            _ = serializer.ReadObject(stream);
        }

        /// <summary>
        /// Deserialization powered by <see cref="System.Xml.Serialization.XmlSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(DESERIALIZATION)]
        public void XmlSerializer2()
        {
            using Stream stream = new MemoryStream(XmlSerializerData);

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            _ = serializer.Deserialize(stream);
        }

        /// <summary>
        /// Deserialization powered by <see cref="System.Xml.Serialization.XmlSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(DESERIALIZATION)]
        public void Utf8Json2()
        {
            using Stream stream = new MemoryStream(Utf8JsonData);

            _ = Utf8JsonSerializer.Deserialize<T>(stream);
        }

        /// <summary>
        /// Deserialization powered by <see cref="BinaryConverter"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(DESERIALIZATION)]
        public void BinaryPack2()
        {
            using Stream stream = new MemoryStream(BinaryPackData);

            _ = BinaryConverter.Deserialize<T>(stream);
        }
    }
}

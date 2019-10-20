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
            for (int i = 0; i < N; i++)
            {
                NewtonsoftStream.Seek(0, SeekOrigin.Begin);
                using StreamReader textReader = new StreamReader(NewtonsoftStream);
                using JsonTextReader jsonReader = new JsonTextReader(textReader);
                var serializer = new Newtonsoft.Json.JsonSerializer();
                _ = serializer.Deserialize<T>(jsonReader);
            }
        }

        /// <summary>
        /// Deserialization powered by <see cref="System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(DESERIALIZATION)]
        public void BinaryFormatter2()
        {
            for (int i = 0; i < N; i++)
            {
                BinaryFormatterStream.Seek(0, SeekOrigin.Begin);
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                _ = formatter.Deserialize(BinaryFormatterStream);
            }
        }

        /// <summary>
        /// Deserialization powered by <see cref="System.Text.Json.JsonSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(DESERIALIZATION)]
        public void NetCoreJson2()
        {
            for (int i = 0; i < N; i++)
            {
                DotNetCoreJsonStream.Seek(0, SeekOrigin.Begin);
                _ = System.Text.Json.JsonSerializer.DeserializeAsync<T>(DotNetCoreJsonStream).Result;
            }
        }

        /// <summary>
        /// Deserialization powered by <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(DESERIALIZATION)]
        public void DataContractJsonSerializer2()
        {
            for (int i = 0; i < N; i++)
            {
                DataContractJsonStream.Seek(0, SeekOrigin.Begin);
                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
                _ = serializer.ReadObject(DataContractJsonStream);
            }
        }

        /// <summary>
        /// Deserialization powered by <see cref="System.Xml.Serialization.XmlSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(DESERIALIZATION)]
        public void XmlSerializer2()
        {
            for (int i = 0; i < N; i++)
            {
                XmlSerializerStream.Seek(0, SeekOrigin.Begin);
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                _ = serializer.Deserialize(XmlSerializerStream);
            }
        }

        /// <summary>
        /// Deserialization powered by <see cref="System.Xml.Serialization.XmlSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(DESERIALIZATION)]
        public void Utf8Json2()
        {
            for (int i = 0; i < N; i++)
            {
                Utf8JsonStream.Seek(0, SeekOrigin.Begin);
                _ = Utf8JsonSerializer.Deserialize<T>(Utf8JsonStream);
            }
        }

        /// <summary>
        /// Deserialization powered by <see cref="BinaryConverter"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(DESERIALIZATION)]
        public void BinaryPack2()
        {
            for (int i = 0; i < N; i++)
            {
                BinaryPackStream.Seek(0, SeekOrigin.Begin);
                _ = BinaryConverter.Deserialize<T>(BinaryPackStream);
            }
        }
    }
}

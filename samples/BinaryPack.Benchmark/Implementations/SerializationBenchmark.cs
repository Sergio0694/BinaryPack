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
    public class SerializationBenchmark<T> where T : class, IInitializable, new()
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

        /// <summary>
        /// Serialization powered by <see cref="Newtonsoft.Json.JsonSerializer"/>
        /// </summary>
        [Benchmark(Baseline = true)]
        [BenchmarkCategory(SERIALIZATION)]
        public void NewtonsoftJson1()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();
                using StreamWriter textWriter = new StreamWriter(stream);
                using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);

                var serializer = new Newtonsoft.Json.JsonSerializer();
                serializer.Serialize(jsonWriter, Model);
                jsonWriter.Flush();
            }
        }

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
        /// Serialization powered by <see cref="System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(SERIALIZATION)]
        public void BinaryFormatter1()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();

                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, Model);
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
        /// Serialization powered by <see cref="System.Text.Json.JsonSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(SERIALIZATION)]
        public void NetCoreJson1()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();
                using Utf8JsonWriter jsonWriter = new Utf8JsonWriter(stream);

                System.Text.Json.JsonSerializer.Serialize(jsonWriter, Model);
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
        /// Serialization powered by <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(SERIALIZATION)]
        public void DataContractJsonSerializer1()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();

                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, Model);
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
        /// Serialization powered by <see cref="System.Xml.Serialization.XmlSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(SERIALIZATION)]
        public void XmlSerializer1()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();

                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                serializer.Serialize(stream, Model);
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
        /// Serialization powered by <see cref="System.Xml.Serialization.XmlSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(SERIALIZATION)]
        public void PortableXaml1()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();

                Portable.Xaml.XamlServices.Save(stream, Model);
            }
        }

        /// <summary>
        /// Serialization powered by <see cref="System.Xml.Serialization.XmlSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(SERIALIZATION)]
        public void Utf8Json1()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();

                Utf8JsonSerializer.Serialize(stream, Model);
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
        /// Serialization powered by <see cref="BinaryConverter"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(SERIALIZATION)]
        public void BinaryPack1()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();

                BinaryConverter.Serialize(Model, stream);
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

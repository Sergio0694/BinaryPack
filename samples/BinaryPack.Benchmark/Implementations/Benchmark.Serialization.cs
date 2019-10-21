using System.IO;
using BenchmarkDotNet.Attributes;
using JsonTextWriter = Newtonsoft.Json.JsonTextWriter;
using Utf8JsonWriter = System.Text.Json.Utf8JsonWriter;
using Utf8JsonSerializer = Utf8Json.JsonSerializer;

namespace BinaryPack.Benchmark.Implementations
{
    public partial class Benchmark<T>
    {
        /// <summary>
        /// Serialization powered by <see cref="Newtonsoft.Json.JsonSerializer"/>
        /// </summary>
        [Benchmark(Baseline = true)]
        [BenchmarkCategory(SERIALIZATION)]
        public void NewtonsoftJson1()
        {
            using Stream stream = new MemoryStream();
            using StreamWriter textWriter = new StreamWriter(stream);
            using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);

            var serializer = new Newtonsoft.Json.JsonSerializer();
            serializer.Serialize(jsonWriter, Model);
            jsonWriter.Flush();
        }

        /// <summary>
        /// Serialization powered by <see cref="System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(SERIALIZATION)]
        public void BinaryFormatter1()
        {
            using Stream stream = new MemoryStream();

            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(stream, Model);
        }

        /// <summary>
        /// Serialization powered by <see cref="System.Text.Json.JsonSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(SERIALIZATION)]
        public void NetCoreJson1()
        {
            using Stream stream = new MemoryStream();
            using Utf8JsonWriter jsonWriter = new Utf8JsonWriter(stream);

            System.Text.Json.JsonSerializer.Serialize(jsonWriter, Model);
        }

        /// <summary>
        /// Serialization powered by <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(SERIALIZATION)]
        public void DataContractJsonSerializer1()
        {
            using Stream stream = new MemoryStream();

            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
            serializer.WriteObject(stream, Model);
        }

        /// <summary>
        /// Serialization powered by <see cref="System.Xml.Serialization.XmlSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(SERIALIZATION)]
        public void XmlSerializer1()
        {
            using Stream stream = new MemoryStream();

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            serializer.Serialize(stream, Model);
        }

        /// <summary>
        /// Serialization powered by <see cref="System.Xml.Serialization.XmlSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(SERIALIZATION)]
        public void PortableXaml1()
        {
            using Stream stream = new MemoryStream();

            Portable.Xaml.XamlServices.Save(stream, Model);
        }

        /// <summary>
        /// Serialization powered by <see cref="System.Xml.Serialization.XmlSerializer"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(SERIALIZATION)]
        public void Utf8Json1()
        {
            using Stream stream = new MemoryStream();

            Utf8JsonSerializer.Serialize(stream, Model);
        }

        /// <summary>
        /// Serialization powered by <see cref="BinaryConverter"/>
        /// </summary>
        [Benchmark]
        [BenchmarkCategory(SERIALIZATION)]
        public void BinaryPack1()
        {
            using Stream stream = new MemoryStream();

            BinaryConverter.Serialize(Model, stream);
        }
    }
}


using System.IO;
using BenchmarkDotNet.Attributes;
using BinaryPack.Models.Interfaces;
using JsonTextWriter = Newtonsoft.Json.JsonTextWriter;
using Utf8JsonWriter = System.Text.Json.Utf8JsonWriter;
using Utf8JsonSerializer = Utf8Json.JsonSerializer;

namespace BinaryPack.Benchmark.Implementations
{
    /// <summary>
    /// A benchmark for a generic type using different serialization libraries
    /// </summary>
    /// <typeparam name="T">The type of model to serialize</typeparam>
    [MemoryDiagnoser]
    public class SerializationBenchmark<T> where T : class, IInitializable, new()
    {
        // Number of iterations to run
        private const int N = 1000;

        private readonly T Model = new T();

        /// <summary>
        /// Initial setup for a benchmarking session
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            Model.Initialize();

            // Newtonsoft
            using (Stream stream = new MemoryStream())
            {
                using StreamWriter textWriter = new StreamWriter(stream);
                using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);

                var serializer = new Newtonsoft.Json.JsonSerializer();
                serializer.Serialize(jsonWriter, Model);
                jsonWriter.Flush();
            }

            // Binary formatter
            using (Stream stream = new MemoryStream())
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, Model);
            }

            // .NETCore JSON
            using (Stream stream = new MemoryStream())
            {
                using Utf8JsonWriter jsonWriter = new Utf8JsonWriter(stream);

                System.Text.Json.JsonSerializer.Serialize(jsonWriter, Model);
            }

            // DataContractJson
            using (Stream stream = new MemoryStream())
            {
                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, Model);
            }

            // XML serializer
            using (Stream stream = new MemoryStream())
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                serializer.Serialize(stream, Model);
            }

            // Portable Xaml
            using (Stream stream = new MemoryStream())
            {
                Portable.Xaml.XamlServices.Save(stream, Model);
            }

            // Utf8Json
            using (Stream stream = new MemoryStream())
            {
                Utf8JsonSerializer.Serialize(stream, Model);
            }

            // BinaryPack
            using (Stream stream = new MemoryStream())
            {
                BinaryConverter.Serialize(Model, stream);
            }
        }

        /// <summary>
        /// Benchmark run powered by <see cref="Newtonsoft.Json.JsonSerializer"/>
        /// </summary>
        [Benchmark(Baseline = true)]
        public void NewtonsoftJson()
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
        /// Benchmark run powered by <see cref="System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>
        /// </summary>
        [Benchmark]
        public void BinaryFormatter()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();

                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, Model);
            }
        }

        /// <summary>
        /// Benchmark run powered by <see cref="System.Text.Json.JsonSerializer"/>
        /// </summary>
        [Benchmark]
        public void NetCoreJson()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();
                using Utf8JsonWriter jsonWriter = new Utf8JsonWriter(stream);

                System.Text.Json.JsonSerializer.Serialize(jsonWriter, Model);
            }
        }

        /// <summary>
        /// Benchmark run powered by <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>
        /// </summary>
        [Benchmark]
        public void DataContractJsonSerializer()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();

                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, Model);
            }
        }

        /// <summary>
        /// Benchmark run powered by <see cref="System.Xml.Serialization.XmlSerializer"/>
        /// </summary>
        [Benchmark]
        public void XmlSerializer()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();

                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                serializer.Serialize(stream, Model);
            }
        }

        /// <summary>
        /// Benchmark run powered by <see cref="System.Xml.Serialization.XmlSerializer"/>
        /// </summary>
        [Benchmark]
        public void PortableXaml()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();

                Portable.Xaml.XamlServices.Save(stream, Model);
            }
        }

        /// <summary>
        /// Benchmark run powered by <see cref="System.Xml.Serialization.XmlSerializer"/>
        /// </summary>
        [Benchmark]
        public void Utf8Json()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();

                Utf8JsonSerializer.Serialize(stream, Model);
            }
        }

        /// <summary>
        /// Benchmark run powered by <see cref="BinaryConverter"/>
        /// </summary>
        [Benchmark]
        public void BinaryPack()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();

                BinaryConverter.Serialize(Model, stream);
            }
        }
    }
}

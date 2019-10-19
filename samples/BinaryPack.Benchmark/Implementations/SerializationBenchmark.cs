using System.IO;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BinaryPack.Models.Interfaces;
using Newtonsoft.Json;
using BinaryFormatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

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
        public void Setup() => Model.Initialize();

        /// <summary>
        /// Benchmark run powered by <see cref="JsonSerializer"/>
        /// </summary>
        [Benchmark(Baseline = true)]
        public void NewtonsoftJson()
        {
            for (int i = 0; i < N; i++)
            {
                using Stream stream = new MemoryStream();
                using StreamWriter textWriter = new StreamWriter(stream);
                using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);

                JsonSerializer serializer = new JsonSerializer();
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

                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, Model);
            }
        }

        /// <summary>
        /// Benchmark run powered by <see cref="System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>
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

using System.IO;
using BenchmarkDotNet.Attributes;
using BinaryPack.Models.Interfaces;
using Newtonsoft.Json;

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

            JsonConvert.SerializeObject(Model);
            BinaryConverter.Serialize(Model);
        }

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

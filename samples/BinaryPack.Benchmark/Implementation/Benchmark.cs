using System;
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
    public partial class Benchmark<T> where T : class, IInitializable, IEquatable<T>, new()
    {
        private const string SERIALIZATION = "Serialization";

        private const string DESERIALIZATION = "Deserialization";

        private readonly T Model = new T();

        private byte[] NewtonsoftJsonData;

        private byte[] BinaryFormatterData;

        private byte[] DotNetCoreJsonData;

        private byte[] DataContractJsonData;

        private byte[] XmlSerializerData;

        private byte[] PortableXamlData;

        private byte[] Utf8JsonData;

        private byte[] MessagePackData;

        private byte[] BinaryPackData;

        /// <summary>
        /// Initial setup for a benchmarking session
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            Model.Initialize();
            T deserializedModel;

            // Newtonsoft
            using (MemoryStream stream = new MemoryStream())
            {
                using StreamWriter textWriter = new StreamWriter(stream);
                using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);

                var serializer = new Newtonsoft.Json.JsonSerializer();
                serializer.Serialize(jsonWriter, Model);
                jsonWriter.Flush();

                NewtonsoftJsonData = stream.GetBuffer();

                stream.Seek(0, SeekOrigin.Begin);
                using StreamReader textReader = new StreamReader(stream);
                using JsonTextReader jsonReader = new JsonTextReader(textReader);
                deserializedModel = serializer.Deserialize<T>(jsonReader);

                if (!Model.Equals(deserializedModel)) throw new InvalidOperationException("Failed comparison with Newtonsoft.Json");
            }

            // Binary formatter
            using (MemoryStream stream = new MemoryStream())
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, Model);

                BinaryFormatterData = stream.GetBuffer();

                stream.Seek(0, SeekOrigin.Begin);
                deserializedModel = (T)formatter.Deserialize(stream);

                if (!Model.Equals(deserializedModel)) throw new InvalidOperationException("Failed comparison with BinaryFormatter");
            }

            // .NETCore JSON
            using (MemoryStream stream = new MemoryStream())
            {
                using Utf8JsonWriter jsonWriter = new Utf8JsonWriter(stream);

                System.Text.Json.JsonSerializer.Serialize(jsonWriter, Model);

                DotNetCoreJsonData = stream.GetBuffer();

                stream.Seek(0, SeekOrigin.Begin);
                deserializedModel = System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream).Result;

                if (!Model.Equals(deserializedModel)) throw new InvalidOperationException("Failed comparison with System.Text.Json");
            }

            // DataContractJson
            using (MemoryStream stream = new MemoryStream())
            {
                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, Model);

                DataContractJsonData = stream.GetBuffer();

                stream.Seek(0, SeekOrigin.Begin);
                deserializedModel = (T)serializer.ReadObject(stream);

                if (!Model.Equals(deserializedModel)) throw new InvalidOperationException("Failed comparison with DataContractJson");
            }

            // XML serializer
            using (MemoryStream stream = new MemoryStream())
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                serializer.Serialize(stream, Model);

                XmlSerializerData = stream.GetBuffer();

                stream.Seek(0, SeekOrigin.Begin);
                deserializedModel = (T)serializer.Deserialize(stream);

                if (!Model.Equals(deserializedModel)) throw new InvalidOperationException("Failed comparison with XmlSerializer");
            }

            // Portable Xaml
            using (MemoryStream stream = new MemoryStream())
            {
                Portable.Xaml.XamlServices.Save(stream, Model);

                PortableXamlData = stream.GetBuffer();

                stream.Seek(0, SeekOrigin.Begin);
                _ = Portable.Xaml.XamlServices.Load(stream);
                if (!Model.Equals(deserializedModel)) throw new InvalidOperationException("Failed comparison with Portable.Xaml");
            }

            // Utf8Json
            using (MemoryStream stream = new MemoryStream())
            {
                Utf8JsonSerializer.Serialize(stream, Model);

                Utf8JsonData = stream.GetBuffer();

                stream.Seek(0, SeekOrigin.Begin);
                deserializedModel = Utf8JsonSerializer.Deserialize<T>(stream);

                if (!Model.Equals(deserializedModel)) throw new InvalidOperationException("Failed comparison with Utf8Json");
            }

            // MessagePack
            using (MemoryStream stream = new MemoryStream())
            {
                MessagePack.MessagePackSerializer.Serialize(stream, Model, MessagePack.Resolvers.ContractlessStandardResolver.Instance);

                MessagePackData = stream.GetBuffer();

                stream.Seek(0, SeekOrigin.Begin);
                deserializedModel = MessagePack.MessagePackSerializer.Deserialize<T>(stream, MessagePack.Resolvers.ContractlessStandardResolver.Instance);

                if (!Model.Equals(deserializedModel)) throw new InvalidOperationException("Failed comparison with MessagePack");
            }

            // BinaryPack
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryConverter.Serialize(Model, stream);

                BinaryPackData = stream.GetBuffer();

                stream.Seek(0, SeekOrigin.Begin);
                deserializedModel = BinaryConverter.Deserialize<T>(stream);

                if (!Model.Equals(deserializedModel)) throw new InvalidOperationException("Failed comparison with BinaryPack");
            }
        }
    }
}

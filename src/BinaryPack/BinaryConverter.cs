using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.IO;
using BinaryPack.Serialization.Processors;
using BinaryWriter = BinaryPack.Serialization.Buffers.BinaryWriter;

namespace BinaryPack
{
    /// <summary>
    /// The entry point <see langword="class"/> for all the APIs in the library
    /// </summary>
    public static class BinaryConverter
    {
        /// <summary>
        /// Serializes the input <typeparamref name="T"/> instance and returns a <see cref="Memory{T}"/> instance
        /// </summary>
        /// <typeparam name="T">The type of instance to serialize</typeparam>
        /// <param name="obj">The input instance to serialize</param>
        /// <returns>A <see cref="Memory{T}"/> instance containing the serialized data</returns>
        public static byte[] Serialize<T>(T obj) where T : new()
        {
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);

            ObjectProcessor<T>.Instance.Serializer(obj, ref writer);

            return writer.Span.ToArray();
        }

        /// <summary>
        /// Serializes the input <typeparamref name="T"/> instance to the target <see cref="Stream"/>
        /// </summary>
        /// <typeparam name="T">The type of instance to serialize</typeparam>
        /// <param name="obj">The input instance to serialize</param>
        /// <param name="stream">The <see cref="Stream"/> instance to use to write the data</param>
        public static void Serialize<T>(T obj, Stream stream) where T : new()
        {
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);

            ObjectProcessor<T>.Instance.Serializer(obj, ref writer);

            stream.Write(writer.Span);
        }

        /// <summary>
        /// Deserializes a <typeparamref name="T"/> instance from the input <see cref="Memory{T}"/> instance
        /// </summary>
        /// <typeparam name="T">The type of instance to deserialize</typeparam>
        /// <param name="memory">The input <see cref="Memory{T}"/> instance to read data from</param>
        [Pure]
        public static unsafe T Deserialize<T>(Memory<byte> memory) where T : new()
        {
            using MemoryHandle handle = memory.Pin();
            using UnmanagedMemoryStream stream = new UnmanagedMemoryStream((byte*)handle.Pointer, memory.Length);

            return Deserialize<T>(stream);
        }

        /// <summary>
        /// Deserializes a <typeparamref name="T"/> instance from the input <see cref="Stream"/> instance
        /// </summary>
        /// <typeparam name="T">The type of instance to deserialize</typeparam>
        /// <param name="stream">The input <see cref="Stream"/> instance to read data from</param>
        [Pure]
        public static T Deserialize<T>(Stream stream) where T : new()
        {
            return ObjectProcessor<T>.Instance.Deserializer(stream);
        }
    }
}

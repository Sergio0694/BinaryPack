using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BinaryPack.Serialization.Processors;
using BinaryReader = BinaryPack.Serialization.Buffers.BinaryReader;
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

            try
            {
                ObjectProcessor<T>.Instance.Serializer(obj, ref writer);

                return writer.Span.ToArray();
            }
            finally
            {
                writer.Dispose();
            }
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

            try
            {
                ObjectProcessor<T>.Instance.Serializer(obj, ref writer);

                stream.Write(writer.Span);
            }
            finally
            {
                writer.Dispose();
            }
        }

        /// <summary>
        /// Deserializes a <typeparamref name="T"/> instance from the input <see cref="Span{T}"/> instance
        /// </summary>
        /// <typeparam name="T">The type of instance to deserialize</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance to read data from</param>
        [Pure]
        public static T Deserialize<T>(Span<byte> span) where T : new()
        {
            BinaryReader reader = new BinaryReader(span);

            return ObjectProcessor<T>.Instance.Deserializer(ref reader);
        }

        /// <summary>
        /// Deserializes a <typeparamref name="T"/> instance from the input <see cref="ReadOnlySpan{T}"/> instance
        /// </summary>
        /// <typeparam name="T">The type of instance to deserialize</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance to read data from</param>
        [Pure]
        public static T Deserialize<T>(ReadOnlySpan<byte> span) where T : new()
        {
            ref readonly byte r0 = ref span[0];
            ref byte r1 = ref Unsafe.AsRef(r0);
            Span<byte> source = MemoryMarshal.CreateSpan(ref r1, span.Length);

            return Deserialize<T>(source);
        }

        /// <summary>
        /// Deserializes a <typeparamref name="T"/> instance from the input <see cref="Memory{T}"/> instance
        /// </summary>
        /// <typeparam name="T">The type of instance to deserialize</typeparam>
        /// <param name="memory">The input <see cref="Memory{T}"/> instance to read data from</param>
        [Pure]
        public static T Deserialize<T>(Memory<byte> memory) where T : new()
        {
            return Deserialize<T>(memory.Span);
        }

        /// <summary>
        /// Deserializes a <typeparamref name="T"/> instance from the input <see cref="ReadOnlyMemory{T}"/> instance
        /// </summary>
        /// <typeparam name="T">The type of instance to deserialize</typeparam>
        /// <param name="memory">The input <see cref="ReadOnlyMemory{T}"/> instance to read data from</param>
        [Pure]
        public static T Deserialize<T>(ReadOnlyMemory<byte> memory) where T : new()
        {
            return Deserialize<T>(memory.Span);
        }

        /// <summary>
        /// Deserializes a <typeparamref name="T"/> instance from the input <see cref="byte"/> array
        /// </summary>
        /// <typeparam name="T">The type of instance to deserialize</typeparam>
        /// <param name="array">The input <see cref="byte"/> array instance to read data from</param>
        [Pure]
        public static T Deserialize<T>(byte[] array) where T : new()
        {
            return Deserialize<T>(array.AsSpan());
        }

        /// <summary>
        /// Deserializes a <typeparamref name="T"/> instance from the input <see cref="Stream"/> instance
        /// </summary>
        /// <typeparam name="T">The type of instance to deserialize</typeparam>
        /// <param name="stream">The input <see cref="Stream"/> instance to read data from</param>
        [Pure]
        public static T Deserialize<T>(Stream stream) where T : new()
        {
            if (stream.CanSeek)
            {
                /* If the stream support the seek operation, we rent a single
                 * array from the array pool, and use a MemoryStream instance
                 * to copy the contents of the input Stream to the memory area of this
                 * rented array. Then we just deserialize the item from that Span<byte> slice. */
                byte[] rent = ArrayPool<byte>.Shared.Rent((int)stream.Length);

                try
                {
                    stream.CopyTo(rent);

                    return Deserialize<T>(rent.AsSpan(0, (int)stream.Length));
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rent);
                }
            }

            /* If the stream doesn't support seeking, we use a BinaryWriter instance
             * to copy the contents of the input Stream, without allocating arrays not
             * from the array pool, which is what an empty MemoryStream would have done. */
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);

            try
            {
                stream.CopyTo(ref writer);

                return Deserialize<T>(writer.Span);
            }
            finally
            {
                writer.Dispose();
            }
        }
    }
}

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BinaryPack.Serialization.Buffers
{
    /// <summary>
    /// A <see langword="struct"/> that provides a fast implementation of a binary reader, pulling data from a given <see cref="byte"/> array
    /// </summary>
    internal struct BinaryReader
    {
        /// <summary>
        /// The <see cref="byte"/> array current in use
        /// </summary>
        private readonly byte[] Buffer;

        /// <summary>
        /// The current position into <see cref="Buffer"/>
        /// </summary>
        private int _Position;

        /// <summary>
        /// Creates a new <see cref="BinaryReader"/> instance with the given parameters
        /// </summary>
        /// <param name="buffer">The source <see cref="byte"/> array to read data from</param>
        public BinaryReader(byte[] buffer)
        {
            Buffer = buffer;
            _Position = 0;
        }

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the underlying buffer
        /// </summary>
        /// <typeparam name="T">The type of value to read from the buffer</typeparam>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            T value = Unsafe.As<byte, T>(ref Buffer[_Position]);
            _Position += Unsafe.SizeOf<T>();

            return value;
        }

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the underlying buffer
        /// </summary>
        /// <typeparam name="T">The type of value to read from the buffer</typeparam>
        /// <param name="value">The value to assign to after reading from the buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(out T value) where T : unmanaged
        {
            value = Unsafe.As<byte, T>(ref Buffer[_Position]);
            _Position += Unsafe.SizeOf<T>();
        }

        /// <summary>
        /// Reads a sequence of elements of type <typeparamref name="T"/> from the underlying buffer
        /// </summary>
        /// <typeparam name="T">The type of elements to read from the buffer</typeparam>
        /// <param name="span">The target <see cref="Span{T}"/> to write the read elements to</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(Span<T> span) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>() * span.Length;
            ref T r0 = ref span.GetPinnableReference();
            ref byte r1 = ref Unsafe.As<T, byte>(ref r0);

            Buffer.AsSpan(_Position, size).CopyTo(MemoryMarshal.CreateSpan(ref r1, size));
            _Position += size;
        }
    }
}

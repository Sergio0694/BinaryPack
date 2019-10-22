using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BinaryPack.Serialization.Buffers
{
    /// <summary>
    /// A <see langword="struct"/> that provides a fast implementation of a binary reader, pulling data from a given <see cref="Span{T}"/>
    /// </summary>
    internal ref struct BinaryReader
    {
        /// <summary>
        /// The <see cref="Span{T}"/> current in use
        /// </summary>
        private readonly Span<byte> Buffer;

        /// <summary>
        /// The current position into <see cref="Buffer"/>
        /// </summary>
        private int _Position;

        /// <summary>
        /// Creates a new <see cref="BinaryReader"/> instance with the given parameters
        /// </summary>
        /// <param name="buffer">The source<see cref="Span{T}"/> to read data from</param>
        public BinaryReader(Span<byte> buffer)
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
            ref byte r0 = ref Buffer.GetPinnableReference();
            ref byte r1 = ref Unsafe.Add(ref r0, _Position);
            T value = Unsafe.As<byte, T>(ref r1);
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
            ref byte r0 = ref Buffer.GetPinnableReference();
            ref byte r1 = ref Unsafe.Add(ref r0, _Position);
            value = Unsafe.As<byte, T>(ref r1);
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
            ref byte r0 = ref Buffer.GetPinnableReference();
            ref byte r1 = ref Unsafe.Add(ref r0, _Position);
            ref T r2 = ref Unsafe.As<byte, T>(ref r1);
            Span<T> source = MemoryMarshal.CreateSpan(ref r2, span.Length);

            source.CopyTo(span);
            _Position += size;
        }
    }
}

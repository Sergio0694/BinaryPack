using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BinaryPack.Extensions;

namespace BinaryPack.Serialization.Buffers
{
    /// <summary>
    /// A <see langword="struct"/> that provides a fast implementation of a binary writer, leveraging <see cref="ArrayPool{T}"/> for memory pooling
    /// </summary>
    internal struct BinaryWriter
    {
        /// <summary>
        /// The <see cref="byte"/> array current in use
        /// </summary>
        private byte[] _Buffer;

        /// <summary>
        /// The current position into <see cref="_Buffer"/>
        /// </summary>
        private int _Position;

        /// <summary>
        /// Creates a new <see cref="BinaryWriter"/> instance with the given parameters
        /// </summary>
        /// <param name="initialSize">The initial size of the internal buffer</param>
        public BinaryWriter(int initialSize = 128)
        {
            _Buffer = ArrayPool<byte>.Shared.Rent(initialSize);
            _Position = 0;
        }

        /// <summary>
        /// Gets a <see cref="ReadOnlySpan{T}"/> instance mapping the used content of the underlying buffer
        /// </summary>
        public ReadOnlySpan<byte> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ReadOnlySpan<byte>(_Buffer, 0, _Position);
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the underlying buffer
        /// </summary>
        /// <typeparam name="T">The type of value to write to the buffer</typeparam>
        /// <param name="value">The <typeparamref name="T"/> value to write to the buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(T value) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();

            EnsureCapacity(size);

            Unsafe.As<byte, T>(ref _Buffer[_Position]) = value;
            _Position += size;
        }

        /// <summary>
        /// Writes the contents of the input <see cref="Span{T}"/> instance to the underlying buffer
        /// </summary>
        /// <typeparam name="T">The type of values to write to the buffer</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> value to write to the buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(Span<T> span) where T : unmanaged
        {
            int
                elementSize = Unsafe.SizeOf<T>(),
                totalSize = elementSize * span.Length;

            EnsureCapacity(totalSize);

            ref T r0 = ref span.GetPinnableReference();
            ref byte r1 = ref Unsafe.As<T, byte>(ref r0);

            MemoryMarshal.CreateSpan(ref r1, totalSize).CopyTo(_Buffer.AsSpan(_Position, totalSize));
            _Position += totalSize;
        }

        /// <summary>
        /// Ensures the buffer in use has the capacity to contain the specified amount of new data
        /// </summary>
        /// <param name="count">The size in bytes of the new data to insert into the buffer</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void EnsureCapacity(int count)
        {
            int
                currentLength = _Buffer.Length,
                requiredLength = _Position + count;

            if (requiredLength <= currentLength) return;

            if (currentLength == 0x7FFFFFC7) throw new InvalidOperationException("Maximum size for a byte[] array exceeded (0x7FFFFFC7), see: https://msdn.microsoft.com/en-us/library/system.array");

            // Calculate the new size of the target array
            int targetLength = requiredLength.UpperBoundLog2();
            if (targetLength < 0) targetLength = 0x7FFFFFC7;

            // Rent the new array and copy the content of the current array
            byte[] rent = ArrayPool<byte>.Shared.Rent(targetLength);
            Buffer.BlockCopy(_Buffer, 0, rent, 0, _Position);

            // Return the old buffer and swap it
            ArrayPool<byte>.Shared.Return(_Buffer);
            _Buffer = rent;
        }
    }
}

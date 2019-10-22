using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BinaryPack.Extensions
{
    /// <summary>
    /// A <see langword="class"/> that provides extension methods for numeric types
    /// </summary>
    public static class NumericExtensions
    {
        /// <summary>
        /// C# no-alloc optimization that maps to the data section, see <see href="https://github.com/dotnet/roslyn/pull/24621"/>
        /// </summary>
        private static ReadOnlySpan<byte> Log2DeBruijn => new byte[]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };

        /// <summary>
        /// Calculates the upper bound of the log base 2 of the input value
        /// </summary>
        /// <param name="n">The input value to compute the bound for</param>
        /// <remarks>Main body pulled from <see href="https://source.dot.net/#System.Private.CoreLib/shared/System/Numerics/BitOperations.cs"/></remarks>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int UpperBoundLog2(this int n)
        {
            uint value = (uint)n - 1;

            // Fill trailing zeros with ones, eg 00010010 becomes 00011111
            value |= value >> 01;
            value |= value >> 02;
            value |= value >> 04;
            value |= value >> 08;
            value |= value >> 16;

            // Compute the log2 and adjust for the upper bound
            return 1 << (Unsafe.AddByteOffset(
                ref MemoryMarshal.GetReference(Log2DeBruijn),
                (IntPtr)(int)((value * 0x07C4ACDDu) >> 27)) + 1);
        }
    }
}

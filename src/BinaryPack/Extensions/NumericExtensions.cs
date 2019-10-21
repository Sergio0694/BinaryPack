using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace BinaryPack.Extensions
{
    /// <summary>
    /// A <see langword="class"/> that provides extension methods for numeric types
    /// </summary>
    internal static class NumericExtensions
    {
        /// <summary>
        /// Calculates the upper bound of the log base 2 of the input value
        /// </summary>
        /// <param name="n">The input value to compute the bound for</param>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int UpperBoundLog2(int n)
        {
            int log = 0;

            if (n > 0xFFFF) { n >>= 16; log = 16; }
            if (n > 0xff) { n >>= 8; log |= 8; }
            if (n > 0xf) { n >>= 4; log |= 4; }
            if (n > 0x3) { n >>= 2; log |= 2; }
            if (n > 0x1) { log |= 1; }

            return 1 << (log + 1);
        }
    }
}

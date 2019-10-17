using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// A <see langword="class"/> that provides extension methods for the <see cref="Type"/> type
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// The <see cref="Dictionary{TKey,TValue}"/> used to retrieve the size of a given type
        /// </summary>
        private static readonly Dictionary<Type, int> SizeMap = new Dictionary<Type, int>();

        /// <summary>
        /// Gets the syze in bytes of the given type
        /// </summary>
        /// <param name="type">The input type to analyze</param>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize(this Type type)
        {
            if (!SizeMap.TryGetValue(type, out int size))
            {
                size = DynamicMethod<Func<int>>.New(il =>
                {
                    il.Emit(OpCodes.Sizeof, type);
                    il.Emit(OpCodes.Ret);
                })();
                SizeMap.Add(type, size);
            }

            return size;
        }
    }
}

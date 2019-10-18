﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BinaryPack.Extensions.System.Reflection.Emit;

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


        private static class _IsUnmanaged<T> where T : unmanaged { }

        /// <summary>
        /// Checks whether or not the input type respects the <see langword="unmanaged"/> constraint
        /// </summary>
        /// <param name="type">The input type to analyze</param>
        [Pure]
        public static bool IsUnmanaged(this Type type)
        {
            try
            {
                _ = typeof(_IsUnmanaged<>).MakeGenericType(type);
                return true;
            }
            catch
            {
                // Not unmanaged
                return false;
            }
        }

        /// <summary>
        /// Gets a sequence of attributes of type <typeparamref name="TAttribute"/> from the members of a given type
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type to look for</typeparam>
        /// <param name="type">The input type to analyze</param>
        [Pure]
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return type.GetMembers().Select(m => m.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault()).OfType<TAttribute>();
        }
    }
}

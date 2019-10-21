using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="ReadOnlySpan{T}"/> type
        /// </summary>
        public static class ReadOnlySpan
        {
            /// <summary>
            /// Gets the <see cref="ReadOnlySpan{T}"/> constructor that takes a generic array
            /// </summary>
            /// <param name="type">The type parameter to use for the target <see cref="ReadOnlySpan{T}"/> type to use</param>
            [Pure]
            public static ConstructorInfo ArrayConstructor(Type type) => (
                from ctor in typeof(ReadOnlySpan<>).MakeGenericType(type).GetConstructors()
                let args = ctor.GetParameters()
                where args.Length == 1 &&
                      args[0].ParameterType == type.MakeArrayType()
                select ctor).First();

            /// <summary>
            /// Gets the <see cref="ReadOnlySpan{T}"/> constructor that takes a generic array, a start index and the length
            /// </summary>
            /// <param name="type">The type parameter to use for the target <see cref="ReadOnlySpan{T}"/> type to use</param>
            [Pure]
            public static ConstructorInfo ArrayWithOffsetAndLengthConstructor(Type type) => (
                from ctor in typeof(ReadOnlySpan<>).MakeGenericType(type).GetConstructors()
                let args = ctor.GetParameters()
                where args.Length == 3 &&
                      args[0].ParameterType == type.MakeArrayType() &&
                      args[1].ParameterType == typeof(int) &&
                      args[2].ParameterType == typeof(int)
                select ctor).First();

            /// <summary>
            /// Gets the <see cref="ReadOnlySpan{T}"/> constructor that takes a <see langword="void"/> pointer and a size
            /// </summary>
            /// <param name="type">The type parameter to use for the target <see cref="ReadOnlySpan{T}"/> type to use</param>
            [Pure]
            public static ConstructorInfo UnsafeConstructor(Type type) => (
                from ctor in typeof(ReadOnlySpan<>).MakeGenericType(type).GetConstructors()
                let args = ctor.GetParameters()
                where args.Length == 2 &&
                      args[0].ParameterType == typeof(void*) &&
                      args[1].ParameterType == typeof(int)
                select ctor).First();

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="ReadOnlySpan{T}"/> indexer getter
            /// </summary>
            /// <param name="type">The type parameter to use for the target <see cref="ReadOnlySpan{T}"/> type to use</param>
            [Pure]
            public static MethodInfo GetterAt(Type type) => (
                from property in typeof(ReadOnlySpan<>).MakeGenericType(type).GetProperties()
                let args = property.GetIndexParameters()
                where args.Length == 1 &&
                      args[0].ParameterType == typeof(int)
                select property.GetMethod).First();
        }
    }
}

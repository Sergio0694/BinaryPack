using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="System.ReadOnlySpan{T}"/> type
        /// </summary>
        public static class ReadOnlySpan
        {
            /// <summary>
            /// Gets the <see cref="ReadOnlySpan{T}"/> constructor that takes a generic array
            /// </summary>
            [Pure]
            public static ConstructorInfo ArrayConstructor(Type type) => (
                from ctor in typeof(ReadOnlySpan<>).MakeGenericType(type).GetConstructors()
                let args = ctor.GetParameters()
                where args.Length == 1 &&
                      args[0].ParameterType == type.MakeArrayType()
                select ctor).First();

            /// <summary>
            /// Gets the <see cref="Syem.ReadOnlySpan{T}"/> constructor that takes a <see langword="void"/> pointer and a size
            /// </summary>
            [Pure]
            public static ConstructorInfo UnsafeConstructor(Type type) => (
                from ctor in typeof(ReadOnlySpan<>).MakeGenericType(type).GetConstructors()
                let args = ctor.GetParameters()
                where args.Length == 2 &&
                      args[0].ParameterType == typeof(void*) &&
                      args[1].ParameterType == typeof(int)
                select ctor).First();
        }
    }
}

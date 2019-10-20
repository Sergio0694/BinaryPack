using System;
using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="System.Span{T}"/> type
        /// </summary>
        public static class Span
        {
            /// <summary>
            /// Gets the <see cref="System.Span{T}"/> constructor that takes a generic array
            /// </summary>
            public static ConstructorInfo ArrayConstructor(Type type) => (
                from ctor in typeof(System.Span<>).MakeGenericType(type).GetConstructors()
                let args = ctor.GetParameters()
                where args.Length == 1 &&
                      args[0].ParameterType == type.MakeArrayType()
                select ctor).First();
        }
    }
}
using System;
using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMethods
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="System.ReadOnlySpan{T}"/> type
        /// </summary>
        public static class ReadOnlySpan
        {
            /// <summary>
            /// Gets the <see cref="System.ReadOnlySpan{T}"/> constructor that takes a <typeparamref name="T"/> array
            /// </summary>
            public static ConstructorInfo ArrayConstructor(Type type) => (
                from ctor in typeof(System.ReadOnlySpan<>).MakeGenericType(type.GetElementType()).GetConstructors()
                let args = ctor.GetParameters()
                where args.Length == 1 &&
                      args[0].ParameterType == type
                select ctor).First();
        }
    }
}

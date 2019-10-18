using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMethods
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="System.ReadOnlySpan{T}"/> type
        /// </summary>
        /// <typeparam name="T">The generic type argument for the target <see cref="System.ReadOnlySpan{T}"/> type</typeparam>
        public static class ReadOnlySpan<T>
        {
            /// <summary>
            /// Gets the <see cref="System.ReadOnlySpan{T}"/> constructor that takes a <see langword="void"/> pointer and a size
            /// </summary>
            public static ConstructorInfo UnsafeConstructor { get; } = (
                from ctor in typeof(System.ReadOnlySpan<T>).GetConstructors()
                let args = ctor.GetParameters()
                where args.Length == 2 &&
                      args[0].ParameterType == typeof(void*) &&
                      args[1].ParameterType == typeof(int)
                select ctor).First();
        }
    }
}

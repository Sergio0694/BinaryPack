using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="System.Span{T}"/> type
        /// </summary>
        /// <typeparam name="T">The generic type argument for the target <see cref="System.Span{T}"/> type</typeparam>
        public static class Span<T>
        {
            /// <summary>
            /// Gets the <see cref="System.Span{T}"/> constructor that takes a <see langword="void"/> pointer and a size
            /// </summary>
            public static ConstructorInfo UnsafeConstructor { get; } = (
                from ctor in typeof(System.Span<T>).GetConstructors()
                let args = ctor.GetParameters()
                where args.Length == 2 &&
                      args[0].ParameterType == typeof(void*) &&
                      args[1].ParameterType == typeof(int)
                select ctor).First();

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="System.Span{T}.GetPinnableReference"/> method
            /// </summary>
            public static MethodInfo GetPinnableReference { get; } = (
                from method in typeof(System.Span<T>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                where method.Name.Equals(nameof(System.Span<T>.GetPinnableReference))
                select method).First(); // Simply calling GetMethod returns null
        }
    }
}

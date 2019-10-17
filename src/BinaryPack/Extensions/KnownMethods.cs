using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BinaryPack.Extensions
{
    /// <summary>
    /// A <see langword="class"/> exposing some frequently used methods for quick lookup
    /// </summary>
    public static class KnownMethods
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
            public static MethodInfo GetPinnableReference { get; } = typeof(Span<T>).GetMethod(nameof(System.Span<T>.GetPinnableReference));
        }

        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="Unsafe"/> type
        /// </summary>
        /// <typeparam name="TFrom">The first type argument for generic methods</typeparam>
        /// <typeparam name="TTo">The second type argument for generic methods</typeparam>
        public static class Unsafe<TFrom, TTo>
        {
            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="Unsafe.As{TFrom,TTo}"/> method
            /// </summary>
            public static MethodInfo As { get; } = (
                from method in typeof(Unsafe).GetMethods()
                where method.Name.Equals(nameof(Unsafe.As)) &&
                      method.GetGenericArguments().Length == 2
                let target = method.MakeGenericMethod(typeof(TFrom), typeof(TTo))
                select target).First();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BinaryPack.Helpers
{
    /// <summary>
    /// A <see langword="class"/> exposing some frequently used methods for quick lookup
    /// </summary>
    public static class KnownMethods
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from the generic types
        /// </summary>
        /// <typeparam name="T">The generic type argument for the target type to work on</typeparam>
        public static class Type<T> where T : new()
        {
            /// <summary>
            /// Gets the default constructor for the type <typeparamref name="T"/>
            /// </summary>
            public static ConstructorInfo DefaultConstructor { get; } = typeof(T).GetConstructor(Type.EmptyTypes);
        }

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
                from ctor in typeof(System.Span<T>).GetConstructors()
                let args = ctor.GetParameters()
                where args.Length == 2 &&
                      args[0].ParameterType == typeof(void*) &&
                      args[1].ParameterType == typeof(int)
                select ctor).First();
        }

        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="Unsafe"/> type
        /// </summary>
        /// <typeparam name="T">The generic type argument for generic methods</typeparam>
        public static class Unsafe<T>
        {
            // Mapping of methods for target type not known at compile time
            private static readonly Dictionary<Type, MethodInfo> _AsMap = new Dictionary<Type, MethodInfo>();

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="Unsafe.As{TFrom,TTo}"/> method
            /// </summary>
            public static MethodInfo As(Type type)
            {
                if (!_AsMap.TryGetValue(type, out MethodInfo methodInfo))
                {
                    methodInfo = (
                        from method in typeof(Unsafe).GetMethods()
                        where method.Name.Equals(nameof(Unsafe.As)) &&
                              method.GetGenericArguments().Length == 2
                        let target = method.MakeGenericMethod(typeof(T), type)
                        select target).First();
                    _AsMap.Add(type, methodInfo);
                }

                return methodInfo;
            }
        }

        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="Unsafe"/> type
        /// </summary>
        public static class Stream
        {
            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="System.IO.Stream.Read(System.Span{byte})"/> method
            /// </summary>
            public static MethodInfo Read { get; } = (
                from method in typeof(System.IO.Stream).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                where method.Name.Equals(nameof(System.IO.Stream.Read))
                let args = method.GetParameters()
                where args.Length == 1
                select method).First();

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="System.IO.Stream.Write(System.ReadOnlySpan{byte})"/> method
            /// </summary>
            public static MethodInfo Write { get; } = (
                from method in typeof(System.IO.Stream).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                where method.Name.Equals(nameof(System.IO.Stream.Write))
                let args = method.GetParameters()
                where args.Length == 1
                select method).First();
        }
    }
}

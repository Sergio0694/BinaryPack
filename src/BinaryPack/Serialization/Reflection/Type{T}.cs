using System;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMethods
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
    }
}

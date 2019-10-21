using System;

namespace BinaryPack.Attributes
{
    /// <summary>
    /// A custom <see cref="Attribute"/> that binds a specific type to a local variable
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class LocalTypeAttribute : Attribute
    {
        /// <summary>
        /// Gets the <see cref="Type"/> value for the current local
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Creates a new <see cref="LocalTypeAttribute"/> instance for the specified type
        /// </summary>
        /// <param name="type">The input <see cref="Type"/> value</param>
        public LocalTypeAttribute(Type type) => Type = type;
    }
}

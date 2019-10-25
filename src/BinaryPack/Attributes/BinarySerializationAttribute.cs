using System;
using BinaryPack.Enums;

namespace BinaryPack.Attributes
{
    /// <summary>
    /// A custom <see cref="Attribute"/> that marks an object that supports serialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class BinarySerializationAttribute : Attribute
    {
        /// <summary>
        /// Gets the serialization mode to use for the target object
        /// </summary>
        public SerializationMode Mode { get; }

        /// <summary>
        /// Creates a new <see cref="BinarySerializationAttribute"/> instance with the specified mode
        /// </summary>
        /// <param name="mode">The requested mode to use to serialize the target object</param>
        public BinarySerializationAttribute(SerializationMode mode = SerializationMode.Properties) => Mode = mode;
    }
}

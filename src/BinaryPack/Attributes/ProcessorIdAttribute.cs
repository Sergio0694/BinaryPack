using System;

namespace BinaryPack.Attributes
{
    /// <summary>
    /// A custom <see cref="Attribute"/> that assigns an id to a given processor
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ProcessorIdAttribute : Attribute
    {
        /// <summary>
        /// Gets the id for the current instance
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Creates a new <see cref="ProcessorIdAttribute"/> instance with the specified id
        /// </summary>
        /// <param name="id">The input id to use</param>
        public ProcessorIdAttribute(int id) => Id = id;
    }
}

using System;

namespace BinaryPack.Enums
{
    /// <summary>
    /// An <see langword="enum"/> that indicates the type of serialization to use for a given object
    /// </summary>
    [Flags]
    public enum SerializationMode
    {
        /// <summary>
        /// Only fields explicitly marked for serialization
        /// </summary>
        Explicit = 0,

        /// <summary>
        /// All the public properties with both setter and a getter
        /// </summary>
        Properties = 1,

        /// <summary>
        /// All the public and not readonly fields
        /// </summary>
        Fields = 2,

        /// <summary>
        /// Includes also non public members respecting the current search criterias
        /// </summary>
        NonPublicMembers = 4,

        /// <summary>
        /// All the members respecting either <see cref="Properties"/> or <see cref="Fields"/> criterias
        /// </summary>
        AllMembers = Properties | Fields,
    }
}

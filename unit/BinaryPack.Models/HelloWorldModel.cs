using System;
using BinaryPack.Models.Interfaces;

#nullable enable

namespace BinaryPack.Models
{
    /// <summary>
    /// A very simple model that simply contains a <see cref="string"/> property
    /// </summary>
    public sealed class HelloWorldModel : IInitializable, IEquatable<HelloWorldModel>
    {
        public string? Property { get; set; }

        public int Value { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            Property = "Hello world";
            Value = 13;
        }

        /// <inheritdoc/>
        public bool Equals(HelloWorldModel other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return (Property == null && other.Property == null ||
                    Property?.Equals(other.Property) == true) &&
                   Value == other.Value;
        }
    }
}

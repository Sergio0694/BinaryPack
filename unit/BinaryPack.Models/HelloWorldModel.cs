using System;
using BinaryPack.Models.Interfaces;

namespace BinaryPack.Models
{
    /// <summary>
    /// A very simple model that simply contains a <see cref="string"/> property
    /// </summary>
    public sealed class HelloWorldModel : IInitializable, IEquatable<HelloWorldModel>
    {
        public string Property { get; set; }

        /// <inheritdoc/>
        public void Initialize() => Property = "Hello world";

        /// <inheritdoc/>
        public bool Equals(HelloWorldModel other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Property.Equals("Hello world");
        }
    }
}

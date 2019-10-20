using System;
using BinaryPack.Models.Interfaces;

#nullable enable

namespace BinaryPack.Models
{
    /// <summary>
    /// A very simple model with a linear hierarchy of instances
    /// </summary>
    [Serializable]
    public sealed class NestedHierarchySimpleModel : IInitializable, IEquatable<NestedHierarchySimpleModel>
    {
        public B? Child { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            Child = new B { Number = 13 };
        }

        /// <inheritdoc/>
        public bool Equals(NestedHierarchySimpleModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                Child == null && other.Child == null ||
                Child?.Equals(other.Child) == true;
        }
    }

    public sealed class B : IEquatable<B>
    {
        public int Number { get; set; }

        /// <inheritdoc/>
        public bool Equals(B? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Number == other.Number;
        }
    }
}

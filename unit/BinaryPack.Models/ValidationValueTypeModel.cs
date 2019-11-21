using System;
using System.Linq;
using BinaryPack.Models.Interfaces;

#nullable enable

namespace BinaryPack.Models
{
    /// <summary>
    /// A value type model that does not respect the <see langword="unmanaged"/> constraint, used to help test the <see cref="ValidationReferenceTypeModel"/> type
    /// </summary>
    public struct ValidationValueTypeModel : IInitializable, IEquatable<ValidationValueTypeModel>, IEquatable<ValidationValueTypeModel?>
    {
        public bool P1 { get; set; }

        public Guid P2 { get; set; }

        public int[]? P3 { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            P1 = true;
            P2 = Guid.NewGuid();
            P3 = Enumerable.Range(0, 128).ToArray();
        }

        /// <inheritdoc/>
        public bool Equals(ValidationValueTypeModel other)
        {
            return
                P1 == other.P1 &&
                P2.Equals(other.P2) &&
                (P3 == null == (other.P3 == null) ||
                 P3?.Zip(other.P3).All(t => t.First == t.Second) == true);
        }

        /// <inheritdoc/>
        public bool Equals(ValidationValueTypeModel? other)
        {
            if (!other.HasValue) return false;
            return Equals(other.Value);
        }
    }
}

using System;
using System.Linq;
using BinaryPack.Models.Interfaces;

namespace BinaryPack.Models
{
    /// <summary>
    /// A very simple model that simply contains a <see cref="DateTime"/> array property
    /// </summary>
    public sealed class UnmanagedArrayModel : IInitializable, IEquatable<UnmanagedArrayModel>
    {
        public DateTime[] Items { get; set; }

        public int Value { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            Items = new[] { DateTime.Now, DateTime.MaxValue, DateTime.MinValue, DateTime.Now, DateTime.Today, DateTime.UtcNow };
            Value = 13;
        }

        /// <inheritdoc/>
        public bool Equals(UnmanagedArrayModel other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Value != other.Value) return false;
            if (Items == null && other.Items == null) return true;
            if (Items == null || other.Items == null) return false;
            if (Items.Length == 0 && other.Items.Length == 0) return true;
            if (Items.Length != other.Items.Length) return false;
            return Items.Zip(other.Items).All(pair => pair.First.Equals(pair.Second));
        }
    }
}

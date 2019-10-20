using System;
using BinaryPack.Models.Interfaces;

namespace BinaryPack.Models
{
    /// <summary>
    /// A very simple model that replicates the sample from <a href="https://msgpack.org/index.html">MessagePack</a>
    /// </summary>
    [Serializable]
    public sealed class MessagePackSampleModel : IInitializable, IEquatable<MessagePackSampleModel>
    {
        public bool Compact { get; set; }

        public int Schema { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            Compact = true;
            Schema = 17;
        }

        /// <inheritdoc/>
        public bool Equals(MessagePackSampleModel other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Compact == other.Compact && Schema == other.Schema;
        }
    }
}

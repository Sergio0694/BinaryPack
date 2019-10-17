using System;

namespace BinaryPack.Models
{
    /// <summary>
    /// A very simple model that replicates the sample from <a href="https://msgpack.org/index.html">MessagePack</a>
    /// </summary>
    public sealed class MessagePackSampleModel : IEquatable<MessagePackSampleModel>
    {
        public bool Compact { get; set; } = true;

        public int Schema { get; set; } = 0;

        /// <inheritdoc/>
        public bool Equals(MessagePackSampleModel other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Compact == other.Compact && Schema == other.Schema;
        }
    }
}

namespace BinaryPack.Models
{
    /// <summary>
    /// A very simple model that replicates the sample from <a href="https://msgpack.org/index.html">MessagePack</a>
    /// </summary>
    public sealed class MessagePackSampleModel
    {
        public bool Compact { get; set; } = true;

        public int Schema { get; set; } = 0;
    }
}

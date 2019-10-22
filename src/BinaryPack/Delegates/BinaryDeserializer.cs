using BinaryPack.Serialization.Buffers;

namespace BinaryPack.Delegates
{
    /// <summary>
    /// A <see langword="delegate"/> that wraps a method to deserialize instances of type <typeparamref name="T"/> from an input <see cref="BinaryReader"/>
    /// </summary>
    /// <typeparam name="T">The type of instances to deserialize</typeparam>
    /// <param name="reader">The input <see cref="BinaryReader"/> to read data from</param>
    internal delegate T BinaryDeserializer<out T>(ref BinaryReader reader);
}

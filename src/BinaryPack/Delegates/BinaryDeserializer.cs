using System.IO;

namespace BinaryPack.Delegates
{
    /// <summary>
    /// A <see langword="delegate"/> that wraps a method to deserialize instances of type <typeparamref name="T"/> from an input <see cref="Stream"/>
    /// </summary>
    /// <typeparam name="T">The type of instances to deserialize</typeparam>
    /// <param name="stream">The input <see cref="Stream"/> to read data from</param>
    internal delegate T BinaryDeserializer<out T>(Stream stream) where T : new();
}

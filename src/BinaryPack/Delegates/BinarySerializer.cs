using BinaryPack.Serialization.Buffers;

namespace BinaryPack.Delegates
{
    /// <summary>
    /// A <see langword="delegate"/> that wraps a method to serialize instances of type <typeparamref name="T"/> to a target <see cref="BinaryWriter"/>
    /// </summary>
    /// <typeparam name="T">The type of instances to serialize</typeparam>
    /// <param name="obj">The input <typeparamref name="T"/> instance to serialize</param>
    /// <param name="writer">The target <see cref="BinaryWriter"/> to write data to</param>
    internal delegate void BinarySerializer<in T>(T obj, ref BinaryWriter writer);
}

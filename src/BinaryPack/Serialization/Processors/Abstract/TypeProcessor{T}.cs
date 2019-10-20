using System.Reflection.Emit;
using BinaryPack.Delegates;
using BinaryPack.Extensions.System.Reflection.Emit;

namespace BinaryPack.Serialization.Processors.Abstract
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for a given type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type of items to serialize and deserialize</typeparam>
    internal abstract class TypeProcessor<T>
    {
        /// <summary>
        /// The <see cref="DynamicMethod{T}"/> instance holding the serializer being built for items of type <typeparamref name="T"/>
        /// </summary>
        public static readonly DynamicMethod<BinarySerializer<T>> SerializerInfo = DynamicMethod<BinarySerializer<T>>.New();

        /// <summary>
        /// The <see cref="DynamicMethod{T}"/> instance holding the deserializer being built for items of type <typeparamref name="T"/>
        /// </summary>
        public static readonly DynamicMethod<BinaryDeserializer<T>> DeserializerInfo = DynamicMethod<BinaryDeserializer<T>>.New();

        /// <summary>
        /// Creates a new <see cref="TypeProcessor{T}"/> instance with the default serializers
        /// </summary>
        protected TypeProcessor()
        {
            Serializer = SerializerInfo.Build(EmitSerializer);
            Deserializer = DeserializerInfo.Build(EmitDeserializer);
        }

        /// <summary>
        /// Gets the <see cref="BinarySerializer{T}"/> instance for items of the current type <typeparamref name="T"/>
        /// </summary>
        public BinarySerializer<T> Serializer { get; }

        /// <summary>
        /// Gets the <see cref="BinaryDeserializer{T}"/> instance for items of the current type <typeparamref name="T"/>
        /// </summary>
        public BinaryDeserializer<T> Deserializer { get; }

        /// <summary>
        /// Emits the instructions for the dynamic serializer method for items of type <typeparamref name="T"/>
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        protected abstract void EmitSerializer(ILGenerator il);

        /// <summary>
        /// Emits the instructions for the dynamic deserializer method for items of type <typeparamref name="T"/>
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        protected abstract void EmitDeserializer(ILGenerator il);
    }
}

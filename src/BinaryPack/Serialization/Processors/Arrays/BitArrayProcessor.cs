using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors.Arrays
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="BitArray"/> instances
    /// </summary>
    internal sealed partial class BitArrayProcessor : TypeProcessor<BitArray?>
    {
        /// <summary>
        /// The <see cref="FieldInfo"/> instance mapping the length of a given <see cref="BitArray"/> instance
        /// </summary>
        private static readonly FieldInfo LengthField = typeof(BitArray).GetField("m_length", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// The <see cref="FieldInfo"/> instance mapping the internal <see cref="int"/> array for a given <see cref="BitArray"/> instance
        /// </summary>
        private static readonly FieldInfo ArrayField = typeof(BitArray).GetField("m_array", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Gets the singleton <see cref="BitArrayProcessor"/> instance to use
        /// </summary>
        public static BitArrayProcessor Instance { get; } = new BitArrayProcessor();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            // if (obj == null) { writer.Write(-1); return; }
            Label isNotNull = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Brtrue_S, isNotNull);
            il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
            il.EmitLoadInt32(-1);
            il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(int)));
            il.Emit(OpCodes.Ret);

            // writer.Write(obj.m_length);
            il.MarkLabel(isNotNull);
            il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitReadMember(LengthField);
            il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(int)));

            // writer.Write(new Span<int>(obj.m_array));
            il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitReadMember(ArrayField);
            il.Emit(OpCodes.Newobj, KnownMembers.Span.ArrayConstructor(typeof(int)));
            il.EmitCall(KnownMembers.BinaryWriter.WriteSpanT(typeof(int)));
            il.Emit(OpCodes.Ret);
        }

        /// <inheritdoc/>
        protected override void EmitDeserializer(ILGenerator il)
        {
            il.DeclareLocals<Locals.Read>();

            // int length = reader.Read<int>();
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitCall(KnownMembers.BinaryReader.ReadT(typeof(int)));
            il.EmitStoreLocal(Locals.Read.Length);

            // if (length == -1) return null;
            Label isNotNull = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.Length);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Bne_Un_S, isNotNull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);

            // BitArray array = new BitArray(length, false);
            il.MarkLabel(isNotNull);
            il.EmitLoadLocal(Locals.Read.Length);
            il.EmitLoadInt32(0);
            il.Emit(OpCodes.Newobj, typeof(BitArray).GetConstructor(new[] { typeof(int), typeof(bool) }));
            il.EmitStoreLocal(Locals.Read.BitArray);

            // reader.Read(new Span<int>(array.m_array));
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitLoadLocal(Locals.Read.BitArray);
            il.EmitReadMember(ArrayField);
            il.Emit(OpCodes.Newobj, KnownMembers.Span.ArrayConstructor(typeof(int)));
            il.EmitCall(KnownMembers.BinaryReader.ReadSpanT(typeof(int)));

            // return array;
            il.EmitLoadLocal(Locals.Read.BitArray);
            il.Emit(OpCodes.Ret);
        }
    }
}

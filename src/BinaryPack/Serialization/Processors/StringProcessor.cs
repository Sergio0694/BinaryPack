using System;
using System.Buffers;
using System.Reflection.Emit;
using System.Text;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for array types
    /// </summary>
    internal sealed partial class StringProcessor : TypeProcessor<string?>
    {
        /// <summary>
        /// Gets the singleton <see cref="StringProcessor"/> instance to use
        /// </summary>
        public static StringProcessor Instance { get; } = new StringProcessor();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            il.DeclareLocals<Locals.Write>();

            // int length = obj?.Length ?? -1;
            Label
                isNotNull = il.DefineLabel(),
                lengthLoaded = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Brtrue_S, isNotNull);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Br_S, lengthLoaded);
            il.MarkLabel(isNotNull);
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitReadMember(typeof(string).GetProperty(nameof(string.Length)));
            il.MarkLabel(lengthLoaded);
            il.EmitStoreLocal(Locals.Write.Length);

            // if (length > 0) length = Encoding.UTF8.GetByteCount(obj.AsSpan());
            Label skipGetByteCount = il.DefineLabel();
            il.EmitLoadLocal(Locals.Write.Length);
            il.EmitLoadInt32(0);
            il.Emit(OpCodes.Ble_S, skipGetByteCount);
            il.EmitReadMember(typeof(Encoding).GetProperty(nameof(Encoding.UTF8)));
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitCall(typeof(MemoryExtensions).GetMethod(nameof(MemoryExtensions.AsSpan), new[] { typeof(string) }));
            il.EmitCallvirt(typeof(Encoding).GetMethod(nameof(Encoding.GetByteCount), new[] { typeof(ReadOnlySpan<char>) }));
            il.EmitStoreLocal(Locals.Write.Length);
            il.MarkLabel(skipGetByteCount);

            // writer.Write(length);
            il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
            il.EmitLoadLocal(Locals.Write.Length);
            il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(int)));

            // if (length > 0) { }
            Label end = il.DefineLabel();
            il.EmitLoadLocal(Locals.Write.Length);
            il.EmitLoadInt32(0);
            il.Emit(OpCodes.Ble_S, end);

            // byte[] array = ArrayPool<byte>.Shared.Rent(length);
            il.EmitReadMember(typeof(ArrayPool<byte>).GetProperty(nameof(ArrayPool<byte>.Shared)));
            il.EmitLoadLocal(Locals.Write.Length);
            il.EmitCallvirt(typeof(ArrayPool<byte>).GetMethod(nameof(ArrayPool<byte>.Rent)));
            il.EmitStoreLocal(Locals.Write.ByteArray);

            // _ = Encoding.UTF8.GetBytes(obj, 0, obj.Length, array, 0);
            il.EmitReadMember(typeof(Encoding).GetProperty(nameof(Encoding.UTF8)));
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitLoadInt32(0);            
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitReadMember(typeof(string).GetProperty(nameof(string.Length)));
            il.EmitLoadLocal(Locals.Write.ByteArray);
            il.EmitLoadInt32(0);
            il.EmitCallvirt(typeof(Encoding).GetMethod(nameof(Encoding.GetBytes), new[] { typeof(string), typeof(int), typeof(int), typeof(byte[]), typeof(int) }));
            il.Emit(OpCodes.Pop);

            // writer.Write(new Span<byte>(array, 0, length));
            il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
            il.EmitLoadLocal(Locals.Write.ByteArray);
            il.EmitLoadInt32(0);
            il.EmitLoadLocal(Locals.Write.Length);
            il.Emit(OpCodes.Newobj, KnownMembers.Span.ArrayWithOffsetAndLengthConstructor(typeof(byte)));
            il.EmitCall(KnownMembers.BinaryWriter.WriteSpanT(typeof(byte)));

            // ArrayPool<byte>.Shared.Return(array);
            il.EmitReadMember(typeof(ArrayPool<byte>).GetProperty(nameof(ArrayPool<byte>.Shared)));
            il.EmitLoadLocal(Locals.Write.ByteArray);
            il.EmitLoadInt32(0);
            il.EmitCallvirt(typeof(ArrayPool<byte>).GetMethod(nameof(ArrayPool<byte>.Return)));

            // return;
            il.MarkLabel(end);
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

            // if (length == 0) return "";
            il.MarkLabel(isNotNull);
            Label notEmpty = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.Length);
            il.Emit(OpCodes.Brtrue_S, notEmpty);
            il.Emit(OpCodes.Ldstr, string.Empty);
            il.Emit(OpCodes.Ret);

            // byte[] array = ArrayPool<byte>.Shared.Rent(length);
            il.MarkLabel(notEmpty);
            il.EmitReadMember(typeof(ArrayPool<byte>).GetProperty(nameof(ArrayPool<byte>.Shared)));
            il.EmitLoadLocal(Locals.Read.Length);
            il.EmitCallvirt(typeof(ArrayPool<byte>).GetMethod(nameof(ArrayPool<byte>.Rent)));
            il.EmitStoreLocal(Locals.Read.ByteArray);

            // reader.Read(new Span<byte>(array, 0, length));
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitLoadLocal(Locals.Read.ByteArray);
            il.EmitLoadInt32(0);
            il.EmitLoadLocal(Locals.Read.Length);
            il.Emit(OpCodes.Newobj, KnownMembers.Span.ArrayWithOffsetAndLengthConstructor(typeof(byte)));
            il.EmitCall(KnownMembers.BinaryReader.ReadSpanT(typeof(byte)));

            // string text = Encoding.UTF8.GetString(p, length);
            il.EmitReadMember(typeof(Encoding).GetProperty(nameof(Encoding.UTF8)));
            il.EmitLoadLocal(Locals.Read.ByteArray);
            il.EmitLoadInt32(0);
            il.EmitLoadLocal(Locals.Read.Length);
            il.EmitCallvirt(typeof(Encoding).GetMethod(nameof(Encoding.GetString), new[] { typeof(byte[]), typeof(int), typeof(int) }));
            il.EmitStoreLocal(Locals.Read.String);

            // ArrayPool<byte>.Shared.Return(array);
            il.EmitReadMember(typeof(ArrayPool<byte>).GetProperty(nameof(ArrayPool<byte>.Shared)));
            il.EmitLoadLocal(Locals.Read.ByteArray);
            il.EmitLoadInt32(0);
            il.EmitCallvirt(typeof(ArrayPool<byte>).GetMethod(nameof(ArrayPool<byte>.Return)));

            // return text;
            il.EmitLoadLocal(Locals.Read.String);
            il.Emit(OpCodes.Ret);
        }
    }
}

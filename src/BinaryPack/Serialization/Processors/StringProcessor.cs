using System;
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

            // Span<byte> span = stackalloc byte[length];
            il.EmitLoadLocal(Locals.Write.Length);
            il.EmitStackalloc();
            il.EmitLoadLocal(Locals.Write.Length);
            il.Emit(OpCodes.Newobj, KnownMembers.Span.UnsafeConstructor(typeof(byte)));
            il.EmitStoreLocal(Locals.Write.SpanByte);

            // _ = Encoding.UTF8.GetBytes(obj.AsSpan(), span);
            il.EmitReadMember(typeof(Encoding).GetProperty(nameof(Encoding.UTF8)));
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitCall(typeof(MemoryExtensions).GetMethod(nameof(MemoryExtensions.AsSpan), new[] { typeof(string) }));
            il.EmitLoadLocal(Locals.Write.SpanByte);
            il.EmitCallvirt(typeof(Encoding).GetMethod(nameof(Encoding.GetBytes), new[] { typeof(ReadOnlySpan<char>), typeof(Span<byte>) }));
            il.Emit(OpCodes.Pop);

            // writer.Write(span);
            il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
            il.EmitLoadLocal(Locals.Write.SpanByte);
            il.EmitCall(KnownMembers.BinaryWriter.WriteSpanT(typeof(byte)));

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

            // byte* p = stackalloc byte[length];
            il.MarkLabel(notEmpty);
            il.EmitLoadLocal(Locals.Read.Length);
            il.EmitStackalloc();
            il.EmitStoreLocal(Locals.Read.BytePtr);

            // reader.Read(new Span<byte>(p, length));
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitLoadLocal(Locals.Read.BytePtr);
            il.EmitLoadLocal(Locals.Read.Length);
            il.Emit(OpCodes.Newobj, KnownMembers.Span.UnsafeConstructor(typeof(byte)));
            il.EmitCall(KnownMembers.BinaryReader.ReadSpanT(typeof(byte)));

            // return Encoding.UTF8.GetString(p, length);
            il.EmitReadMember(typeof(Encoding).GetProperty(nameof(Encoding.UTF8)));
            il.EmitLoadLocalAddress(Locals.Read.BytePtr);
            il.EmitLoadLocal(Locals.Read.Length);
            il.EmitCallvirt(typeof(Encoding).GetMethod(nameof(Encoding.GetString), new[] { typeof(byte*), typeof(int) }));
            il.Emit(OpCodes.Ret);
        }
    }
}

using System.Reflection.Emit;
using BinaryPack.Extensions.System.Reflection.Emit;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Extensions;
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

            // if (obj == null) { } else { }
            Label
                notNull = il.DefineLabel(),
                serialize = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Brtrue_S, notNull);

            // void* p = stackalloc byte[4]; *p = -1; length = 0;
            il.EmitStackalloc(typeof(int));
            il.EmitStoreLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadInt32(-1);
            il.EmitStoreToAddress(typeof(int));
            il.EmitLoadInt32(0);
            il.EmitStoreLocal(Locals.Write.Length);
            il.Emit(OpCodes.Br_S, serialize);

            // if (obj.Property.Length == 0) { } else { }
            Label notEmpty = il.DefineLabel();
            il.MarkLabel(notNull);
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitReadMember(KnownMembers.String.Length);
            il.Emit(OpCodes.Brtrue_S, notEmpty);

            // void* p = stackalloc byte[4]; *p = 0; size = 0;
            il.EmitStackalloc(typeof(int));
            il.EmitStoreLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadInt32(0);
            il.EmitStoreToAddress(typeof(int));
            il.EmitLoadInt32(0);
            il.EmitStoreLocal(Locals.Write.Length);
            il.Emit(OpCodes.Br_S, serialize);

            // void* p = stackalloc byte[Encoding.UTF8.GetByteCount(obj.AsSpan()) + 4];
            il.MarkLabel(notEmpty);
            il.EmitReadMember(KnownMembers.Encoding.UTF8);
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitCall(OpCodes.Call, KnownMembers.String.AsSpan, null);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Encoding.GetByteCount, null);
            il.Emit(OpCodes.Dup);
            il.EmitStoreLocal(Locals.Write.Length);
            il.EmitLoadInt32(sizeof(int));
            il.Emit(OpCodes.Add);
            il.EmitStackalloc();
            il.EmitStoreLocal(Locals.Write.BytePtr);

            // *p = size;
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.Length);
            il.EmitStoreToAddress(typeof(int));

            // _ = Encoding.UTF8.GetBytes(obj.AsSpan(), new Span<byte>(p + 4, size);
            il.EmitReadMember(KnownMembers.Encoding.UTF8);
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitCall(OpCodes.Call, KnownMembers.String.AsSpan, null);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitAddOffset(sizeof(int));
            il.EmitLoadLocal(Locals.Write.Length);
            il.Emit(OpCodes.Newobj, KnownMembers.Span<byte>.UnsafeConstructor);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Encoding.GetBytes, null);
            il.Emit(OpCodes.Pop);

            // stream.Write(new ReadOnlySpan<byte>(p, size + 4));
            il.MarkLabel(serialize);
            il.EmitLoadArgument(Arguments.Write.Stream);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.Length);
            il.EmitLoadInt32(sizeof(int));
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Newobj, KnownMembers.ReadOnlySpan<byte>.UnsafeConstructor);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Write, null);
            il.Emit(OpCodes.Ret);
        }

        /// <inheritdoc/>
        protected override void EmitDeserializer(ILGenerator il)
        {
            il.DeclareLocals<Locals.Read>();

            // Span<byte> span = stackalloc byte[4];
            il.EmitStackalloc(typeof(int));
            il.EmitLoadInt32(sizeof(int));
            il.Emit(OpCodes.Newobj, KnownMembers.Span<byte>.UnsafeConstructor);
            il.EmitStoreLocal(Locals.Read.SpanByte);

            // _ = stream.Read(span);
            il.EmitLoadArgument(Arguments.Read.Stream);
            il.EmitLoadLocal(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Read, null);
            il.Emit(OpCodes.Pop);

            // int size = Unsafe.As<byte, int>(ref span.GetPinnableReference());
            il.EmitLoadLocalAddress(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Call, KnownMembers.Span<byte>.GetPinnableReference, null);
            il.Emit(OpCodes.Ldind_I4);
            il.EmitStoreLocal(Locals.Read.Length);

            // if (size == -1) return null;
            Label notNull = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.Length);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Bne_Un_S, notNull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);

            // if (size == 0) return "";
            il.MarkLabel(notNull);
            Label notEmpty = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.Length);
            il.Emit(OpCodes.Brtrue_S, notEmpty);
            il.Emit(OpCodes.Ldstr, string.Empty);
            il.Emit(OpCodes.Ret);

            // span = stackalloc byte[size];
            il.MarkLabel(notEmpty);
            il.EmitLoadLocal(Locals.Read.Length);
            il.EmitStackalloc();
            il.EmitLoadLocal(Locals.Read.Length);
            il.Emit(OpCodes.Newobj, KnownMembers.Span<byte>.UnsafeConstructor);
            il.EmitStoreLocal(Locals.Read.SpanByte);

            // _ = stream.Read(span);
            il.EmitLoadArgument(Arguments.Read.Stream);
            il.EmitLoadLocal(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Read, null);
            il.Emit(OpCodes.Pop);

            // return Encoding.UTF8.GetString(&span.GetPinnableReference(), size);
            il.EmitReadMember(KnownMembers.Encoding.UTF8);
            il.EmitLoadLocalAddress(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Call, KnownMembers.Span<byte>.GetPinnableReference, null);
            il.EmitLoadLocal(Locals.Read.Length);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Encoding.GetString, null);
            il.Emit(OpCodes.Ret);
        }
    }
}

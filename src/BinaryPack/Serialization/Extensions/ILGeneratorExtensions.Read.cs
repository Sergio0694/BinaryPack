using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using BinaryPack.Extensions.System.Reflection.Emit;
using BinaryPack.Helpers;
using BinaryPack.Serialization.Constants;

namespace BinaryPack.Serialization.Extensions
{
    /// <summary>
    /// A <see langword="class"/> that provides serialization extension methods for the <see langword="ILGenerator"/> type
    /// </summary>
    internal static partial class ILGeneratorExtensions
    {
        /// <summary>
        /// Emits the necessary instructions to deserialize an <see langword="unmanaged"/> value to a target <see cref="System.IO.Stream"/> instance
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="property">The property to deserialize</param>
        public static void EmitDeserializeUnmanagedProperty(this ILGenerator il, PropertyInfo property)
        {
            // Span<byte> span = stackalloc byte[Unsafe.SizeOf<TProperty>()];
            il.EmitStackalloc(property.PropertyType);
            il.EmitLoadInt32(property.PropertyType.GetSize());
            il.Emit(OpCodes.Newobj, KnownMethods.Span<byte>.UnsafeConstructor);
            il.EmitStoreLocal(Locals.Read.SpanByte);

            // _ = stream.Read(span);
            il.EmitLoadArgument(Arguments.Read.Stream);
            il.EmitLoadLocal(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Callvirt, KnownMethods.Stream.Read, null);
            il.Emit(OpCodes.Pop);

            // obj.Property = Unsafe.As<byte, TProperty>(ref span.GetPinnableReference());
            il.EmitLoadLocal(Locals.Read.Obj);
            il.EmitLoadLocalAddress(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Call, KnownMethods.Span<byte>.GetPinnableReference, null);
            il.EmitLoadFromAddress(property.PropertyType);
            il.EmitWriteMember(property);
        }

        /// <summary>
        /// Emits the necessary instructions to deserialize a <see cref="string"/> value to a target <see cref="System.IO.Stream"/> instance
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="property">The property to deserialize</param>
        public static void EmitDeserializeStringProperty(this ILGenerator il, PropertyInfo property)
        {
            // Span<byte> span = stackalloc byte[4];
            il.EmitStackalloc(typeof(int));
            il.EmitLoadInt32(sizeof(int));
            il.Emit(OpCodes.Newobj, KnownMethods.Span<byte>.UnsafeConstructor);
            il.EmitStoreLocal(Locals.Read.SpanByte);

            // _ = stream.Read(span);
            il.EmitLoadArgument(Arguments.Read.Stream);
            il.EmitLoadLocal(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Callvirt, KnownMethods.Stream.Read, null);
            il.Emit(OpCodes.Pop);

            // int size = Unsafe.As<byte, int>(ref span.GetPinnableReference());
            il.EmitLoadLocalAddress(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Call, KnownMethods.Span<byte>.GetPinnableReference, null);
            il.Emit(OpCodes.Ldind_I4);
            il.EmitStoreLocal(Locals.Read.Int);

            // if (size != -1) { }
            Label end = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.Int);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brtrue_S, end);

            // span = stackalloc byte[size];
            il.EmitLoadLocal(Locals.Read.Int);
            il.EmitStackalloc();
            il.EmitLoadLocal(Locals.Read.Int);
            il.Emit(OpCodes.Newobj, KnownMethods.Span<byte>.UnsafeConstructor);
            il.EmitStoreLocal(Locals.Read.SpanByte);

            // _ = stream.Read(span);
            il.EmitLoadArgument(Arguments.Read.Stream);
            il.EmitLoadLocal(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Callvirt, KnownMethods.Stream.Read, null);
            il.Emit(OpCodes.Pop);

            // obj.Property = Encoding.UTF8.GetString(&span.GetPinnableReference(), size);
            il.EmitLoadLocal(Locals.Read.Obj);
            il.EmitReadMember(typeof(Encoding).GetProperty(nameof(Encoding.UTF8)));
            il.EmitLoadLocalAddress(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Call, KnownMethods.Span<byte>.GetPinnableReference, null);
            il.EmitLoadLocal(Locals.Read.Int);
            il.EmitCall(OpCodes.Callvirt, KnownMethods.Encoding.GetString, null);
            il.EmitWriteMember(property);
            il.MarkLabel(end);
        }
    }
}

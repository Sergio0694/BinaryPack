using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using BinaryPack.Extensions;
using BinaryPack.Extensions.System.Reflection.Emit;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Extensions
{
    /// <summary>
    /// A <see langword="class"/> that provides serialization extension methods for the <see langword="ILGenerator"/> type
    /// </summary>
    internal static partial class ILGeneratorExtensions
    {
        /// <summary>
        /// Emits the necessary instructions to deserialize an <see langword="unmanaged"/> value from an input <see cref="System.IO.Stream"/> instance
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="property">The property to deserialize</param>
        public static void EmitDeserializeUnmanagedProperty(this ILGenerator il, PropertyInfo property)
        {
            // Span<byte> span = stackalloc byte[Unsafe.SizeOf<TProperty>()];
            il.EmitStackalloc(property.PropertyType);
            il.EmitLoadInt32(property.PropertyType.GetSize());
            il.Emit(OpCodes.Newobj, KnownMembers.Span<byte>.UnsafeConstructor);
            il.EmitStoreLocal(Locals.Read.SpanByte);

            // _ = stream.Read(span);
            il.EmitLoadArgument(Arguments.Read.Stream);
            il.EmitLoadLocal(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Read, null);
            il.Emit(OpCodes.Pop);

            // obj.Property = Unsafe.As<byte, TProperty>(ref span.GetPinnableReference());
            il.EmitLoadLocal(Locals.Read.T);
            il.EmitLoadLocalAddress(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Call, KnownMembers.Span<byte>.GetPinnableReference, null);
            il.EmitLoadFromAddress(property.PropertyType);
            il.EmitWriteMember(property);
        }

        /// <summary>
        /// Emits the necessary instructions to deserialize a <see cref="string"/> value from an input <see cref="System.IO.Stream"/> instance
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="property">The property to deserialize</param>
        public static void EmitDeserializeStringProperty(this ILGenerator il, PropertyInfo property)
        {
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
            il.EmitStoreLocal(Locals.Read.Int);

            // if (size == -1) { } else { }
            Label end = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.Int);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brtrue_S, end);

            // if (size == 0) { obj.Property = ""; } else { }
            Label notEmpty = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.Int);
            il.Emit(OpCodes.Brtrue_S, notEmpty);
            il.EmitLoadLocal(Locals.Read.T);
            il.Emit(OpCodes.Ldstr, string.Empty);
            il.EmitWriteMember(property);
            il.Emit(OpCodes.Br_S, end);

            // span = stackalloc byte[size];
            il.MarkLabel(notEmpty);
            il.EmitLoadLocal(Locals.Read.Int);
            il.EmitStackalloc();
            il.EmitLoadLocal(Locals.Read.Int);
            il.Emit(OpCodes.Newobj, KnownMembers.Span<byte>.UnsafeConstructor);
            il.EmitStoreLocal(Locals.Read.SpanByte);

            // _ = stream.Read(span);
            il.EmitLoadArgument(Arguments.Read.Stream);
            il.EmitLoadLocal(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Read, null);
            il.Emit(OpCodes.Pop);

            // obj.Property = Encoding.UTF8.GetString(&span.GetPinnableReference(), size);
            il.EmitLoadLocal(Locals.Read.T);
            il.EmitReadMember(typeof(Encoding).GetProperty(nameof(Encoding.UTF8)));
            il.EmitLoadLocalAddress(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Call, KnownMembers.Span<byte>.GetPinnableReference, null);
            il.EmitLoadLocal(Locals.Read.Int);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Encoding.GetString, null);
            il.EmitWriteMember(property);
            il.MarkLabel(end);
        }

        /// <summary>
        /// Emits the necessary instructions to deserialize an array of <see langword="unmanaged"/> values from an input <see cref="System.IO.Stream"/> instance
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="property">The property to serialize</param>
        public static void EmitDeserializeUnmanagedArrayProperty(this ILGenerator il, PropertyInfo property)
        {
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
            il.EmitStoreLocal(Locals.Read.Int);

            // if (size == -1) { } else { }
            Label end = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.Int);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brtrue_S, end);

            // if (size == 0) { obj.Property = Array.Empty<T>(); } else { }
            Label notEmpty = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.Int);
            il.Emit(OpCodes.Brtrue_S, notEmpty);
            il.EmitLoadLocal(Locals.Read.T);
            il.EmitCall(OpCodes.Call, KnownMembers.Array.Empty(property.PropertyType.GetElementType()), null);
            il.EmitWriteMember(property);
            il.Emit(OpCodes.Br_S, end);

            // object obj = new T[size];
            il.MarkLabel(notEmpty);
            il.EmitLoadLocal(Locals.Read.Int);
            il.Emit(OpCodes.Newarr, property.PropertyType.GetElementType());
            il.EmitStoreLocal(Locals.Read.Obj);

            // _ = stream.Read(MemoryMarshal.AsBytes(new Span<T>((T[])obj)));
            il.EmitLoadArgument(Arguments.Read.Stream);
            il.EmitLoadLocal(Locals.Read.Obj);
            il.Emit(OpCodes.Castclass, property.PropertyType);
            il.Emit(OpCodes.Newobj, KnownMembers.Span.ArrayConstructor(property.PropertyType));
            il.EmitCall(OpCodes.Call, KnownMembers.MemoryMarshal.AsByteSpan(property.PropertyType.GetElementType()), null);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Read, null);
            il.Emit(OpCodes.Pop);

            // obj.Property = (T[])obj;
            il.EmitLoadLocal(Locals.Read.T);
            il.EmitLoadLocal(Locals.Read.Obj);
            il.Emit(OpCodes.Castclass, property.PropertyType);
            il.EmitWriteMember(property);
            il.MarkLabel(end);
        }

        /// <summary>
        /// Emits the necessary instructions to load the default instance of the target type
        /// </summary>
        /// <typeparam name="T">The type of instance being deserialized</typeparam>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        public static void EmitDeserializeEmptyInstanceOrNull<T>(this ILGenerator il) where T : new()
        {
            // Span<byte> span = stackalloc byte[1];
            il.EmitStackalloc(typeof(byte));
            il.EmitLoadInt32(sizeof(byte));
            il.Emit(OpCodes.Newobj, KnownMembers.Span<byte>.UnsafeConstructor);
            il.EmitStoreLocal(Locals.Read.SpanByte);

            // _ = stream.Read(span);
            il.EmitLoadArgument(Arguments.Read.Stream);
            il.EmitLoadLocal(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Read, null);
            il.Emit(OpCodes.Pop);

            // byte isNotNull = span.GetPinnableReference();
            il.EmitLoadLocalAddress(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Call, KnownMembers.Span<byte>.GetPinnableReference, null);
            il.EmitLoadFromAddress(typeof(byte));

            // T obj = isNotNull ? new T() : null;
            Label
                isNotNull = il.DefineLabel(),
                end = il.DefineLabel();
            il.Emit(OpCodes.Brtrue_S, isNotNull);
            il.EmitLoadLocalAddress(Locals.Read.T);
            il.Emit(OpCodes.Initobj, typeof(T));
            il.Emit(OpCodes.Br_S, end);
            il.MarkLabel(isNotNull);
            il.Emit(OpCodes.Newobj, KnownMembers.Type<T>.DefaultConstructor);
            il.EmitStoreLocal(Locals.Read.T);
            il.MarkLabel(end);
        }
    }
}

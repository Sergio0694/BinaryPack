using System.Reflection;
using System.Reflection.Emit;
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
        /// Emits the necessary instructions to serialize an <see langword="unmanaged"/> value to a target <see cref="System.IO.Stream"/> instance
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="property">The property to serialize</param>
        public static void EmitSerializeUnmanagedProperty(this ILGenerator il, PropertyInfo property)
        {
            // byte* p = stackalloc byte[Unsafe.SizeOf<TProperty>()];
            il.EmitStackalloc(property.PropertyType);
            il.EmitStoreLocal(Locals.Write.BytePtr);

            // Unsafe.Write<TProperty>(p, obj.Property);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadArgument(Arguments.Write.Obj);
            il.EmitReadMember(property);
            il.EmitStoreToAddress(property.PropertyType);

            // stream.Write(new ReadOnlySpan<byte>(p, Unsafe.SizeOf<TProperty>()));
            il.EmitLoadArgument(Arguments.Write.Stream);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadInt32(property.PropertyType.GetSize());
            il.Emit(OpCodes.Newobj, KnownMembers.ReadOnlySpan<byte>.UnsafeConstructor);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Write, null);
        }

        /// <summary>
        /// Emits the necessary instructions to serialize a <see cref="string"/> value to a target <see cref="System.IO.Stream"/> instance
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="property">The property to serialize</param>
        public static void EmitSerializeStringProperty(this ILGenerator il, PropertyInfo property)
        {
            // if (obj.Property == null) { } else { }
            Label
                notNull = il.DefineLabel(),
                serialize = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.Obj);
            il.EmitReadMember(property);
            il.Emit(OpCodes.Brtrue_S, notNull);

            // void* p = stackalloc byte[4]; *p = -1; size = 0;
            il.EmitStackalloc(typeof(int));
            il.EmitStoreLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadInt32(-1);
            il.EmitStoreToAddress(typeof(int));
            il.EmitLoadInt32(0);
            il.EmitStoreLocal(Locals.Write.Int);
            il.Emit(OpCodes.Br_S, serialize);

            // if (obj.Property.Length == 0) { } else { }
            Label notEmpty = il.DefineLabel();
            il.MarkLabel(notNull);
            il.EmitLoadArgument(Arguments.Write.Obj);
            il.EmitReadMember(property);
            il.EmitReadMember(KnownMembers.String.Length);
            il.Emit(OpCodes.Brtrue_S, notEmpty);

            // void* p = stackalloc byte[4]; *p = 0; size = 0;
            il.EmitStackalloc(typeof(int));
            il.EmitStoreLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadInt32(0);
            il.EmitStoreToAddress(typeof(int));
            il.EmitLoadInt32(0);
            il.EmitStoreLocal(Locals.Write.Int);
            il.Emit(OpCodes.Br_S, serialize);

            // void* p = stackalloc byte[Encoding.UTF8.GetByteCount(obj.Property.AsSpan()) + 4];
            il.MarkLabel(notEmpty);
            il.EmitReadMember(KnownMembers.Encoding.UTF8);
            il.EmitLoadArgument(Arguments.Write.Obj);
            il.EmitReadMember(property);
            il.EmitCall(OpCodes.Call, KnownMembers.String.AsSpan, null);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Encoding.GetByteCount, null);
            il.Emit(OpCodes.Dup);
            il.EmitStoreLocal(Locals.Write.Int);
            il.EmitLoadInt32(sizeof(int));
            il.Emit(OpCodes.Add);
            il.EmitStackalloc();
            il.EmitStoreLocal(Locals.Write.BytePtr);

            // *p = size;
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.Int);
            il.EmitStoreToAddress(typeof(int));

            // _ = Encoding.UTF8.GetBytes(obj.Property.AsSpan(), new Span<byte>(p + 4, size);
            il.EmitReadMember(KnownMembers.Encoding.UTF8);
            il.EmitLoadArgument(Arguments.Write.Obj);
            il.EmitReadMember(property);
            il.EmitCall(OpCodes.Call, KnownMembers.String.AsSpan, null);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitAddOffset(sizeof(int));
            il.EmitLoadLocal(Locals.Write.Int);
            il.Emit(OpCodes.Newobj, KnownMembers.Span<byte>.UnsafeConstructor);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Encoding.GetBytes, null);
            il.Emit(OpCodes.Pop);

            // stream.Write(new ReadOnlySpan<byte>(p, size + 4));
            il.MarkLabel(serialize);
            il.EmitLoadArgument(Arguments.Write.Stream);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.Int);
            il.EmitLoadInt32(sizeof(int));
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Newobj, KnownMembers.ReadOnlySpan<byte>.UnsafeConstructor);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Write, null);
        }

        /// <summary>
        /// Emits the necessary instructions to serialize an array of <see langword="unmanaged"/> values to a target <see cref="System.IO.Stream"/> instance
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="property">The property to serialize</param>
        public static void EmitSerializeUnmanagedArrayProperty(this ILGenerator il, PropertyInfo property)
        {
            // int size = obj.Property?.Length ?? -1;
            Label
                notNull = il.DefineLabel(),
                lengthLoaded = il.DefineLabel();
            il.EmitStackalloc(typeof(int));
            il.EmitStoreLocal(Locals.Write.BytePtr);
            il.EmitLoadArgument(Arguments.Write.Obj);
            il.EmitReadMember(property);
            il.Emit(OpCodes.Brtrue_S, notNull);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Br_S, lengthLoaded);
            il.MarkLabel(notNull);
            il.EmitLoadArgument(Arguments.Write.Obj);
            il.EmitReadMember(property);
            il.EmitReadMember(KnownMembers.Array.Length);
            il.MarkLabel(lengthLoaded);
            il.EmitStoreLocal(Locals.Write.Int);

            // void* p = stackalloc byte[4]; *p = size;
            il.EmitStackalloc(typeof(int));
            il.EmitStoreLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.Int);
            il.EmitStoreToAddress(typeof(int));

            // stream.Write(new ReadOnlySpan<byte>(p, 4));
            il.EmitLoadArgument(Arguments.Write.Stream);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadInt32(sizeof(int));
            il.Emit(OpCodes.Newobj, KnownMembers.ReadOnlySpan<byte>.UnsafeConstructor);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Write, null);

            // if (size > 0) { }
            Label end = il.DefineLabel();
            il.EmitLoadLocal(Locals.Write.Int);
            il.EmitLoadInt32(0);
            il.Emit(OpCodes.Cgt);
            il.Emit(OpCodes.Brfalse_S, end);

            // stream.Write(MemoryMarshal.AsBytes(new ReadOnlySpan(obj.Property)));
            il.EmitLoadArgument(Arguments.Write.Stream);
            il.EmitLoadArgument(Arguments.Write.Obj);
            il.EmitReadMember(property);
            il.Emit(OpCodes.Newobj, KnownMembers.ReadOnlySpan.ArrayConstructor(property.PropertyType));
            il.EmitCall(OpCodes.Call, KnownMembers.MemoryMarshal.AsBytes(property.PropertyType.GetElementType()), null);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Write, null);

            il.MarkLabel(end);
        }
    }
}
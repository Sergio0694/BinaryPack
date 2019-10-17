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
        /// Emits the necessary instructions to serialize an <see langword="unmanaged"/> value to a target <see cref="System.IO.Stream"/> instance
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="property">The property to serialize</param>
        public static void EmitSerializeUnmanagedProperty(this ILGenerator il, PropertyInfo property)
        {
            il.EmitStackalloc(property.PropertyType);
            il.EmitStoreLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.Emit(OpCodes.Ldarg_0); // The input object is always argument 0
            il.EmitReadMember(property);
            il.EmitStoreToAddress(property.PropertyType);
            il.Emit(OpCodes.Ldarg_1); // The target stream is argument 1
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadInt32(property.PropertyType.GetSize());
            il.Emit(OpCodes.Newobj, KnownMethods.ReadOnlySpan<byte>.UnsafeConstructor);
            il.EmitCall(OpCodes.Callvirt, KnownMethods.Stream.Write, null);
        }

        /// <summary>
        /// Emits the necessary instructions to serialize a <see cref="string"/> value to a target <see cref="System.IO.Stream"/> instance
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="property">The property to serialize</param>
        public static void EmitSerializeStringProperty(this ILGenerator il, PropertyInfo property)
        {
            // void* p = stackalloc byte[Encoding.UTF8.GetByteCount(obj.Property)];
            il.Emit(OpCodes.Ldarg_0);
            il.EmitReadMember(property);
            il.EmitReadMember(typeof(Encoding).GetProperty(nameof(Encoding.UTF8)));
            il.EmitCall(OpCodes.Callvirt, KnownMethods.Encoding.GetByteCount, null);
            il.Emit(OpCodes.Dup);
            il.EmitStoreLocal(Locals.Write.Int);
            il.EmitStackalloc();
            il.EmitStoreLocal(Locals.Write.BytePtr);

            // _ = Encoding.UTF8.GetBytes(obj.Property.AsSpan(), new Span<byte>(p, size);
            il.EmitReadMember(typeof(Encoding).GetProperty(nameof(Encoding.UTF8)));
            il.Emit(OpCodes.Ldarg_0);
            il.EmitReadMember(property);
            il.Emit(OpCodes.Newobj, KnownMethods.String.AsSpan);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.Int);
            il.Emit(OpCodes.Newobj, KnownMethods.Span<byte>.UnsafeConstructor);
            il.EmitCall(OpCodes.Callvirt, KnownMethods.Encoding.GetBytes, null);
            il.Emit(OpCodes.Pop);

            // stream.Write(new Span<byte>(p, size));'
            il.Emit(OpCodes.Ldarg_1);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.Int);
            il.Emit(OpCodes.Newobj, KnownMethods.ReadOnlySpan<byte>.UnsafeConstructor);
            il.EmitCall(OpCodes.Callvirt, KnownMethods.Stream.Write, null);
        }
    }
}
using System;
using System.Reflection;
using System.Reflection.Emit;
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
            il.EmitStackalloc(property.PropertyType);
            il.EmitLoadInt32(property.PropertyType.GetSize());
            il.Emit(OpCodes.Newobj, KnownMethods.Span<byte>.UnsafeConstructor);
            il.EmitStoreLocal(Locals.Read.SpanByte);
            il.Emit(OpCodes.Ldarg_0);
            il.EmitLoadLocal(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Callvirt, KnownMethods.Stream.Read, null);
            il.Emit(OpCodes.Pop);
            il.EmitLoadLocal(Locals.Read.SpanByte);
            il.Emit(OpCodes.Ldloca_S, 1);
            il.EmitCall(OpCodes.Call, KnownMethods.Span<byte>.GetPinnableReference, null);
            il.EmitLoadFromAddress(property.PropertyType);
            il.EmitWriteMember(property);
        }
    }
}

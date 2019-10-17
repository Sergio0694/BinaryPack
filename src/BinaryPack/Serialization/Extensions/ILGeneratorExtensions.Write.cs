using System;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Extensions.System.Reflection.Emit;
using BinaryPack.Helpers;

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
            il.EmitStoreLocal(0); // Local byte* variable is local 0
            il.EmitLoadLocal(0);
            il.Emit(OpCodes.Ldarg_0); // The input object is always argument 0
            il.EmitReadMember(property);
            il.EmitStoreToAddress(property.PropertyType);
            il.Emit(OpCodes.Ldarg_1); // The target stream is argument 1
            il.EmitLoadLocal(0);
            il.EmitLoadInt32(property.PropertyType.GetSize());
            il.Emit(OpCodes.Newobj, KnownMethods.ReadOnlySpan<byte>.UnsafeConstructor);
            il.EmitCall(OpCodes.Callvirt, KnownMethods.Stream.Write, null);
        }
    }
}
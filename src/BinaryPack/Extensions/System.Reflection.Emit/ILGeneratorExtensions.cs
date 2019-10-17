using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BinaryPack.Extensions.System.Reflection.Emit
{
    /// <summary>
    /// A <see langword="class"/> that provides extension methods for the <see langword="ILGenerator"/> type
    /// </summary>
    internal static class ILGeneratorExtensions
    {
        /// <summary>
        /// Puts the appropriate <see langword="unbox"/> or <see langword="castclass"/> instruction to unbox/cast a value onto the stream of instructions
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="type">The type of value to convert</param>
        public static void EmitCastOrUnbox(this ILGenerator il, Type type)
        {
            il.Emit(type.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, type);
        }

        /// <summary>
        /// Puts the appropriate <see langword="ldarg"/> instruction to read an argument onto the stream of instructions
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="index">The index of the argument to load</param>
        public static void EmitLoadArgument(this ILGenerator il, int index)
        {
            if (index <= 3)
            {
                il.Emit(index switch
                {
                    0 => OpCodes.Ldarg_0,
                    1 => OpCodes.Ldarg_1,
                    2 => OpCodes.Ldarg_2,
                    3 => OpCodes.Ldarg_3,
                    _ => throw new InvalidOperationException($"Invalid argument index [{index}]")
                });
            }
            else if (index <= 255) il.Emit(OpCodes.Ldarg_S, (byte)index);
            else if (index <= 65534) il.Emit(OpCodes.Ldarg, (short)index);
            else throw new ArgumentOutOfRangeException($"Invalid argument index {index}");
        }

        /// <summary>
        /// Puts the appropriate <see langword="ldloc"/> instruction to read a local variable onto the stream of instructions
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="index">The index of the local variable to load</param>
        public static void EmitLoadLocal(this ILGenerator il, int index)
        {
            if (index <= 3)
            {
                il.Emit(index switch
                {
                    0 => OpCodes.Ldloc_0,
                    1 => OpCodes.Ldloc_1,
                    2 => OpCodes.Ldloc_2,
                    3 => OpCodes.Ldloc_3,
                    _ => throw new InvalidOperationException($"Invalid local variable index [{index}]")
                });
            }
            else if (index <= 255) il.Emit(OpCodes.Ldloc_S, (byte)index);
            else if (index <= 65534) il.Emit(OpCodes.Ldloc, (short)index);
            else throw new ArgumentOutOfRangeException($"Invalid local index {index}");
        }

        /// <summary>
        /// Puts the appropriate <see langword="stloc"/> instruction to write a local variable onto the stream of instructions
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="index">The index of the local variable to store</param>
        public static void EmitStoreLocal(this ILGenerator il, int index)
        {
            if (index <= 3)
            {
                il.Emit(index switch
                {
                    0 => OpCodes.Stloc_0,
                    1 => OpCodes.Stloc_1,
                    2 => OpCodes.Stloc_2,
                    3 => OpCodes.Stloc_3,
                    _ => throw new InvalidOperationException($"Invalid local variable index [{index}]")
                });
            }
            else if (index <= 255) il.Emit(OpCodes.Stloc_S, (byte)index);
            else if (index <= 65534) il.Emit(OpCodes.Stloc, (short)index);
            else throw new ArgumentOutOfRangeException($"Invalid local index {index}");
        }

        /// <summary>
        /// Puts the appropriate <see langword="ldsfld"/>, <see langword="ldfld"/>, <see langword="call"/> or <see langword="callvirt"/> instruction to read a member on the stream of instructions
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="member">The member to read</param>
        public static void EmitReadMember(this ILGenerator il, MemberInfo member)
        {
            switch (member)
            {
                case FieldInfo field:
                    il.Emit(field.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, field);
                    break;
                case PropertyInfo property when property.CanRead:
                    il.EmitCall(property.GetMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, property.GetMethod, null);
                    break;
                default: throw new ArgumentException($"The input {member.GetType()} instance can't be read");
            }
        }

        /// <summary>
        /// Puts the appropriate <see langword="ldsfld"/>, <see langword="ldfld"/>, <see langword="call"/> or <see langword="callvirt"/> instruction to write a member on the stream of instructions
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="member">The member to write</param>
        public static void EmitWriteMember(this ILGenerator il, MemberInfo member)
        {
            switch (member)
            {
                case FieldInfo field:
                    il.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
                    break;
                case PropertyInfo property when property.CanRead:
                    il.EmitCall(property.GetMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, property.SetMethod, null);
                    break;
                default: throw new ArgumentException($"The input {member.GetType()} instance can't be written");
            }
        }

        /// <summary>
        /// Puts the appropriate <see langword="ldc.i4"/> instruction to load an <see langword="int"/> value on the execution stack
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="value">The value to place on top of the execution stack</param>
        public static void EmitLoadInt32(this ILGenerator il, int value)
        {
            // Push the value to the stack
            if (value < -128) il.Emit(OpCodes.Ldc_I4, value);
            else if (value < 0) il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
            else if (value <= 8)
            {
                il.Emit(value switch
                {
                    0 => OpCodes.Ldc_I4_0,
                    1 => OpCodes.Ldc_I4_1,
                    2 => OpCodes.Ldc_I4_2,
                    3 => OpCodes.Ldc_I4_3,
                    4 => OpCodes.Ldc_I4_4,
                    5 => OpCodes.Ldc_I4_5,
                    6 => OpCodes.Ldc_I4_6,
                    7 => OpCodes.Ldc_I4_7,
                    8 => OpCodes.Ldc_I4_8,
                    _ => throw new InvalidOperationException($"Invalid value [{value}]")
                });
            }
            else if (value <= 127) il.Emit(OpCodes.Ldc_I4_S, (byte)value);
            else il.Emit(OpCodes.Ldc_I4, value);
        }

        /// <summary>
        /// Puts the appropriate <see langword="ldc.i4"/>, <see langword="conv.i"/> and <see langword="add"/> instructions to advance a reference onto the stream of instructions
        /// </summary>
        /// <typeparam name="T">The type of reference at the top of the stack</typeparam>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="offset">The offset to use to advance the current reference on top of the execution stack</param>
        public static void EmitAddOffset<T>(this ILGenerator il, int offset) => il.EmitAddOffset(Unsafe.SizeOf<T>() * offset);

        /// <summary>
        /// Puts the appropriate <see langword="ldc.i4"/>, <see langword="conv.i"/> and <see langword="add"/> instructions to advance a reference onto the stream of instructions
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="offset">The offset in bytes to use to advance the current reference on top of the execution stack</param>
        public static void EmitAddOffset(this ILGenerator il, int offset)
        {
            il.EmitLoadInt32(offset);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Add);
        }

        /// <summary>
        /// Puts the appropriate <see langword="ldind"/> or <see langword="ldobj"/> instruction to read from a reference onto the stream of instructions
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="type">The type of value being read from the current reference on top of the execution stack</param>
        public static void EmitLoadFromAddress(this ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                // Pick the optimal opcode to set a value type
                OpCode opcode = Marshal.SizeOf(type) switch
                {
                    // Use the faster op codes for sizes <= 8
                    1 when type == typeof(bool) || type == typeof(byte) => OpCodes.Ldind_U1,
                    1 when type == typeof(sbyte) => OpCodes.Ldind_I1,
                    2 when type == typeof(short) => OpCodes.Ldind_I2,
                    2 when type == typeof(ushort) => OpCodes.Ldind_U2,
                    4 when type == typeof(float) => OpCodes.Ldind_R4,
                    4 when type == typeof(int) => OpCodes.Ldind_I4,
                    4 when type == typeof(uint) => OpCodes.Ldind_U4,
                    8 when type == typeof(double) => OpCodes.Ldind_R8,
                    8 when type == typeof(long) || type == typeof(ulong) => OpCodes.Ldind_I8,

                    // Default to ldobj for all other value types
                    _ => OpCodes.Ldobj
                };

                // Also pass the type token if ldobj is used
                if (opcode == OpCodes.Ldobj) il.Emit(opcode, type);
                else il.Emit(opcode);
            }
            else il.Emit(OpCodes.Ldind_Ref);
        }

        /// <summary>
        /// Puts the appropriate <see langword="stind"/> or <see langword="stobj"/> instruction to write to a reference onto the stream of instructions
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="type">The type of value being written to the current reference on top of the execution stack</param>
        public static void EmitStoreToAddress(this ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                // Pick the optimal opcode to set a value type
                OpCode opcode = Marshal.SizeOf(type) switch
                {
                    // Use the faster op codes for sizes <= 8
                    1 when type == typeof(bool) || type == typeof(byte) || type == typeof(sbyte) => OpCodes.Stind_I1,
                    2 when type == typeof(short) || type == typeof(ushort) => OpCodes.Stind_I2,
                    4 when type == typeof(float) => OpCodes.Stind_R4,
                    4 when type == typeof(int) || type == typeof(uint) => OpCodes.Stind_I4,
                    8 when type == typeof(double) => OpCodes.Stind_R8,
                    8 when type == typeof(long) || type == typeof(ulong) => OpCodes.Stind_I8,

                    // Default to stobj for all other value types
                    _ => OpCodes.Stobj
                };

                // Also pass the type token if stobj is used
                if (opcode == OpCodes.Stobj) il.Emit(opcode, type);
                else il.Emit(opcode);
            }
            else il.Emit(OpCodes.Stind_Ref);
        }

        /// <summary>
        /// Loads a buffer of type <see cref="byte"/> onto the execution stack, through the use of <see langword="stackalloc"/>
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="type">The type of item for the buffer to create</param>
        /// <param name="size">The number of items of the specified type to fit onto the created buffer</param>
        public static void EmitStackalloc(this ILGenerator il, Type type, int size = 1)
        {
            il.EmitLoadInt32(type.GetSize() * size);
            il.EmitStackalloc();
        }

        /// <summary>
        /// Loads a buffer of type <see cref="byte"/> onto the execution stack, through the use of <see langword="stackalloc"/>
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <remarks>This method assumes that the size in bytes of the buffer to allocate is on top of the execution stack</remarks>
        public static void EmitStackalloc(this ILGenerator il)
        {
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Localloc);
        }
    }
}

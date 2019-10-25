using System.Diagnostics.Contracts;
using System.Linq;
using BinaryPack.Attributes;

namespace System.Reflection.Emit
{
    /// <summary>
    /// A <see langword="class"/> that provides extension methods for the <see langword="ILGenerator"/> type
    /// </summary>
    internal static class ILGeneratorExtensions
    {
        /// <summary>
        /// Declares a local variable with the name specified by the given <typeparamref name="T"/> value
        /// </summary>
        /// <typeparam name="T">The type to use to retrieve the type of local to declare</typeparam>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="local">The name of the local variable to declare</param>
        public static void DeclareLocal<T>(this ILGenerator il, T local) where T : Enum
        {
            FieldInfo fieldInfo = typeof(T).GetField(local.ToString(), BindingFlags.Public | BindingFlags.Static);
            LocalTypeAttribute attribute = fieldInfo.GetCustomAttribute<LocalTypeAttribute>();
            il.DeclareLocal(attribute.Type);
        }

        /// <summary>
        /// Declares local variables with the types specified in the public members of a given type
        /// </summary>
        /// <typeparam name="T">The type to use to retrieve the types of locals to declare</typeparam>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        public static void DeclareLocals<T>(this ILGenerator il) where T : Enum
        {
            foreach (Type type in
                from field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static)
                let attribute = field.GetCustomAttributes<LocalTypeAttribute>().FirstOrDefault()
                where attribute != null
                select attribute.Type)
            {
                il.DeclareLocal(type);
            }
        }

        /// <summary>
        /// Emits the necessary instructions to execute a <see langword="call"/> operation onto the stream of instructions
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> instance representing the method to invoke</param>
        public static void EmitCall(this ILGenerator il, MethodInfo methodInfo) => il.EmitCall(OpCodes.Call, methodInfo, null);

        /// <summary>
        /// Emits the necessary instructions to execute a <see langword="callvirt"/> operation onto the stream of instructions
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> instance representing the method to invoke</param>
        public static void EmitCallvirt(this ILGenerator il, MethodInfo methodInfo) => il.EmitCall(OpCodes.Callvirt, methodInfo, null);

        /// <summary>
        /// Puts the appropriate <see langword="ldarg"/> instruction to read an argument onto the stream of instructions
        /// </summary>
        /// <typeparam name="T">The type of index to use</typeparam>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="index">The index of the argument to load</param>
        public static void EmitLoadArgument<T>(this ILGenerator il, T index) where T : Enum => il.EmitLoadArgument((int)(object)index);

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
        /// <typeparam name="T">The type of index to use</typeparam>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="index">The index of the local variable to load</param>
        public static void EmitLoadLocal<T>(this ILGenerator il, T index) where T : Enum => il.EmitLoadLocal((int)(object)index);

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
        /// Puts the appropriate <see langword="ldloca"/> instruction to read a local variable address onto the stream of instructions
        /// </summary>
        /// <typeparam name="T">The type of index to use</typeparam>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="index">The index of the local variable to load the address for</param>
        public static void EmitLoadLocalAddress<T>(this ILGenerator il, T index) where T : Enum => il.EmitLoadLocalAddress((int)(object)index);

        /// <summary>
        /// Puts the appropriate <see langword="ldloca"/> instruction to read a local variable address onto the stream of instructions
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="index">The index of the local variable to load the address for</param>
        public static void EmitLoadLocalAddress(this ILGenerator il, int index)
        {
            if (index <= 255) il.Emit(OpCodes.Ldloca_S, (byte)index);
            else if (index <= 65534) il.Emit(OpCodes.Ldloca, (short)index);
            else throw new ArgumentOutOfRangeException($"Invalid local index {index}");
        }

        /// <summary>
        /// Puts the appropriate <see langword="stloc"/> instruction to write a local variable onto the stream of instructions
        /// </summary>
        /// <typeparam name="T">The type of index to use</typeparam>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="index">The index of the local variable to store</param>
        public static void EmitStoreLocal<T>(this ILGenerator il, T index) where T : Enum => il.EmitStoreLocal((int)(object)index);

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
                    if (property.GetMethod.IsStatic || property.DeclaringType.IsValueType) il.EmitCall(property.GetMethod);
                    else il.EmitCallvirt(property.GetMethod);
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
                case FieldInfo field when !field.IsInitOnly:
                    il.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
                    break;
                case PropertyInfo property when property.CanWrite:
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
            else if (value < -1) il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
            else if (value == -1) il.Emit(OpCodes.Ldc_I4_M1);
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
        /// Puts the appropriate <see langword="conv.i"/> and <see langword="add"/> instructions to advance a reference onto the stream of instructions
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="type">The type of value being read from the current reference on top of the execution stack</param>
        public static void EmitAddOffset(this ILGenerator il, Type type)
        {
            il.EmitLoadInt32(type.GetSize());
            il.Emit(OpCodes.Mul);
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
                OpCode opcode = type.GetSize() switch
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
                OpCode opcode = type.GetSize() switch
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
        /// <remarks>This method assumes that the size in bytes of the buffer to allocate is on top of the execution stack</remarks>
        public static void EmitStackalloc(this ILGenerator il)
        {
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Localloc);
        }

        /// <summary>
        /// Creates a new try block scope that is automatically closed when it's not needed anymore
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <returns>An <see cref="IDisposable"/> instance that's responsible for closing the try scope</returns>
        [Pure]
        public static IDisposable EmitTryBlockScope(this ILGenerator il)
        {
            il.BeginExceptionBlock();

            return new TryBlock(il);
        }

        /// <summary>
        /// A <see langword="class"/> used as a proxy for the <see cref="EmitTryBlock"/> method
        /// </summary>
        private sealed class TryBlock : IDisposable
        {
            /// <summary>
            /// The <see cref="ILGenerator"/> instance currently in use
            /// </summary>
            private readonly ILGenerator IL;

            public TryBlock(ILGenerator il) => IL = il;

            /// <inheritdoc/>
            public void Dispose() => IL.EndExceptionBlock();
        }
    }
}

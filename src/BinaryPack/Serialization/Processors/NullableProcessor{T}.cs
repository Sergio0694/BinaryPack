using System;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="Nullable{T}"/> types
    /// </summary>
    internal sealed partial class NullableProcessor<T> : TypeProcessor<T?> where T : struct
    {
        /// <summary>
        /// The <see cref="FieldInfo"/> instance mapping the private <see cref="Nullable{T}"/> field indicating whether or not the value is not <see langword="null"/>
        /// </summary>
        private static readonly FieldInfo HasValueField = typeof(T?).GetField("hasValue", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// The <see cref="FieldInfo"/> instance mapping the private <see cref="Nullable{T}"/> field with the actual <typeparamref name="T"/> value
        /// </summary>
        private static readonly FieldInfo ValueField = typeof(T?).GetField("value", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Gets the singleton <see cref="NullableProcessor{T}"/> instance to use
        /// </summary>
        public static NullableProcessor<T> Instance { get; } = new NullableProcessor<T>();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            /* This processor has a special path for bool? variables. In order to save one
             * extra byte written to the stream, it leverages the fact that bool variables
             * can only have the value of either 0 or 1, and serializes null values as -1.
             * To do so, the serialized value is being written to the writer as a sbyte. */
            if (typeof(T) == typeof(bool))
            {
                // if (!obj.hasValue) writer.Write<sbyte>(-1);
                Label
                    isNotNull = il.DefineLabel(),
                    write = il.DefineLabel();
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitLoadArgument(Arguments.Write.T);
                il.EmitReadMember(HasValueField);
                il.Emit(OpCodes.Brtrue_S, isNotNull);
                il.EmitLoadInt32(-1);
                il.Emit(OpCodes.Br_S, write);

                // else writer.Write<sbyte>(obj.value);
                il.MarkLabel(isNotNull);
                il.EmitLoadArgument(Arguments.Write.T);
                il.EmitReadMember(ValueField);
                il.MarkLabel(write);
                il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(sbyte)));
            }
            else
            {
                /* For all other struct types, first a bool is written to the stream to
                 * indicate whether the input value is null or not. If it is null, the
                 * rest of the serialization is skipped. If it is not, there are two cases
                 * to consider: if T is unmanaged, the whole value is just written directly
                 * to the writer, otherwise the right ObjectProcessor<T> instanced is invoked. */
                il.DeclareLocals<Locals.Write>();

                // writer.Write(hasValue = obj.hasValue);
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitLoadArgument(Arguments.Write.T);
                il.EmitReadMember(HasValueField);
                il.Emit(OpCodes.Dup);
                il.EmitStoreLocal(Locals.Write.HasValue);
                il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(bool)));

                // if (hasValue) { }
                Label isNull = il.DefineLabel();
                il.EmitLoadLocal(Locals.Write.HasValue);
                il.Emit(OpCodes.Brfalse_S, isNull);

                if (typeof(T).IsUnmanaged())
                {
                    // writer.Write(obj.value);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(ValueField);
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(T)));
                }
                else
                {
                    // TypeProcessor<T>.Serializer(obj.value, ref writer);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(ValueField);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(T)));
                }

                il.MarkLabel(isNull);
            }

            // return;
            il.Emit(OpCodes.Ret);
        }

        /// <inheritdoc/>
        protected override void EmitDeserializer(ILGenerator il)
        {
            il.DeclareLocal(typeof(T?));

            if (typeof(T) == typeof(bool))
            {
                // sbyte value = reader.Read<sbyte>();
                il.DeclareLocal(Locals.Read.NullableBoolAsSignedByte);
                il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                il.EmitCall(KnownMembers.BinaryReader.ReadT(typeof(sbyte)));
                il.EmitStoreLocal(Locals.Read.NullableBoolAsSignedByte);

                // return value == -1 ? default(bool?) : Unsafe.As<sbyte, bool>(ref value);
                Label
                    isNotNull = il.DefineLabel(),
                    end = il.DefineLabel();
                il.EmitLoadLocal(Locals.Read.NullableBoolAsSignedByte);
                il.EmitLoadInt32(-1);
                il.Emit(OpCodes.Bne_Un_S, isNotNull);
                il.EmitLoadLocalAddress(Locals.Read.NullableT);
                il.Emit(OpCodes.Initobj, typeof(T?));
                il.EmitLoadLocal(Locals.Read.NullableT);
                il.Emit(OpCodes.Br_S, end);
                il.MarkLabel(isNotNull);
                il.EmitLoadLocal(Locals.Read.NullableBoolAsSignedByte);
                il.Emit(OpCodes.Conv_U1);
                il.Emit(OpCodes.Newobj, typeof(T?).GetConstructor(new[] { typeof(T) }));
                il.MarkLabel(end);
            }
            else
            {
                // if (!reader.Read<bool>()) return default(T?);
                Label
                    isNotNull = il.DefineLabel(),
                    end = il.DefineLabel();
                il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                il.EmitCall(KnownMembers.BinaryReader.ReadT(typeof(bool)));
                il.Emit(OpCodes.Brtrue_S, isNotNull);
                il.EmitLoadLocalAddress(Locals.Read.NullableT);
                il.Emit(OpCodes.Initobj, typeof(T?));
                il.EmitLoadLocal(Locals.Read.NullableT);
                il.Emit(OpCodes.Br_S, end);

                // else return reader.Read<T>();
                il.MarkLabel(isNotNull);
                il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                il.EmitCall(KnownMembers.BinaryReader.ReadT(typeof(T)));
                il.Emit(OpCodes.Newobj, typeof(T?).GetConstructor(new[] { typeof(T) }));
                il.MarkLabel(end);
            }

            // return;
            il.Emit(OpCodes.Ret);
        }
    }
}

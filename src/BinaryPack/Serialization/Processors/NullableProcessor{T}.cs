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
                il.EmitReadMember(typeof(T?).GetField("hasValue", BindingFlags.NonPublic));
                il.Emit(OpCodes.Brtrue_S, isNotNull);
                il.EmitLoadInt32(-1);
                il.Emit(OpCodes.Br_S, write);

                // else writer.Write<sbyte>(obj.value);
                il.MarkLabel(isNotNull);
                il.EmitLoadArgument(Arguments.Write.T);
                il.EmitReadMember(typeof(T?).GetField("value", BindingFlags.NonPublic));
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
                il.EmitReadMember(typeof(T?).GetField("hasValue", BindingFlags.NonPublic));
                il.Emit(OpCodes.Dup);
                il.EmitStoreLocal(Locals.Write.HasValue);
                il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(bool)));

                // if (hasValue) { }
                Label isNull = il.DefineLabel();
                il.EmitLoadArgument(Locals.Write.HasValue);
                il.Emit(OpCodes.Brfalse_S, isNull);

                if (typeof(T).IsUnmanaged())
                {
                    // writer.Write(obj.value);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(typeof(T?).GetField("value", BindingFlags.NonPublic));
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(T)));
                }
                else
                {
                    // ObjectProcessor<T>.Serialize(obj.value, ref writer);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(typeof(T?).GetField("value", BindingFlags.NonPublic));
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(ObjectProcessor<>), typeof(T)));
                }

                il.MarkLabel(isNull);
            }

            // return;
            il.Emit(OpCodes.Ret);
        }

        /// <inheritdoc/>
        protected override void EmitDeserializer(ILGenerator il)
        {
            throw new NotImplementedException();
        }
    }
}


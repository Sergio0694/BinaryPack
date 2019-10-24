using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="IEnumerable{T}"/> types
    /// </summary>
    /// <typeparam name="T">The type of items in <see cref="IEnumerable{T}"/> instances to serialize and deserialize</typeparam>
    internal sealed partial class IEnumerableProcessor<T> : TypeProcessor<IEnumerable<T>?>
    {
        /// <summary>
        /// Gets the singleton <see cref="IEnumerableProcessor{T}"/> instance to use
        /// </summary>
        public static IEnumerableProcessor<T> Instance { get; } = new IEnumerableProcessor<T>();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            il.DeclareLocal(typeof(IEnumerator<T>));

            // writer.Write(obj != null);
            Label
                isNotNull = il.DefineLabel(),
                isNullLoaded = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Brtrue_S);
            il.EmitLoadInt32(0);
            il.Emit(OpCodes.Br_S, isNullLoaded);
            il.MarkLabel(isNotNull);
            il.EmitLoadInt32(1);
            il.MarkLabel(isNullLoaded);
            il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(bool)));

            // if (object == null) return;
            Label enumeration = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Brtrue_S, enumeration);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(enumeration);

            // using IEnumerator<T> enumerator = obj.GetEnumerator();
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitCallvirt(typeof(IEnumerable<T>).GetMethod(nameof(IEnumerable<T>.GetEnumerator)));
            il.EmitStoreLocal(Locals.Write.IEnumeratorT);
            using (il.EmitTryBlockScope())
            {
                Label moveNext = il.DefineLabel();
                il.Emit(OpCodes.Br_S, moveNext);

                // writer.Write(true);
                Label loop = il.DefineLabel();
                il.MarkLabel(loop);
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitLoadInt32(1);
                il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(bool)));

                // T item = enumerator.Current
                il.EmitLoadLocal(Locals.Write.IEnumeratorT);
                il.EmitReadMember(typeof(IEnumerator<T>).GetProperty(nameof(IEnumerator<T>.Current)));

                // writer.Write(item);
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(T)));

                // while (enumerator.MoveNext()) { }
                il.MarkLabel(moveNext);
                il.EmitLoadLocal(Locals.Write.IEnumeratorT);
                il.EmitCallvirt(typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext)));
                il.Emit(OpCodes.Brtrue_S, loop);

                // writer.Write(false);
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitLoadInt32(0);
                il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(bool)));

                // finally { enumerator?.Dispose(); }
                il.BeginFinallyBlock();
                Label endFinally = il.DefineLabel();
                il.EmitLoadLocal(Locals.Write.IEnumeratorT);
                il.Emit(OpCodes.Brfalse_S, endFinally);
                il.EmitLoadLocal(Locals.Write.IEnumeratorT);
                il.EmitCallvirt(typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose)));
                il.MarkLabel(endFinally);
            }

            // return;
            il.Emit(OpCodes.Ret);
        }

        /// <inheritdoc/>
        protected override void EmitDeserializer(ILGenerator il)
        {
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);
        }
    }
}

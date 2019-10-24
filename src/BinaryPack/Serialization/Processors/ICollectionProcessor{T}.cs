using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using BinaryPack.Attributes;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="IEnumerable{T}"/> types
    /// </summary>
    /// <typeparam name="T">The type of items in <see cref="IEnumerable{T}"/> instances to serialize and deserialize</typeparam>
    [ProcessorId(2)]
    internal sealed partial class ICollectionProcessor<T> : TypeProcessor<ICollection<T>?>
    {
        /// <summary>
        /// Gets the singleton <see cref="IEnumerableProcessor{T}"/> instance to use
        /// </summary>
        public static ICollectionProcessor<T> Instance { get; } = new ICollectionProcessor<T>();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            il.DeclareLocal(typeof(IEnumerator<T>));

            // writer.Write(obj?.Count ?? -1);
            Label
                isNotNull = il.DefineLabel(),
                countLoaded = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Brtrue_S, isNotNull);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Br_S, countLoaded);
            il.MarkLabel(isNotNull);
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitReadMember(typeof(ICollection<T>).GetProperty(nameof(ICollection<T>.Count)));
            il.MarkLabel(countLoaded);
            il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(int)));

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
                // Loop start
                Label
                    loop = il.DefineLabel(),
                    moveNext = il.DefineLabel();
                il.Emit(OpCodes.Br_S, moveNext);
                il.MarkLabel(loop);

                /* Same item serialization used in the IEnumerableProcessor<T> type: we write
                 * unmanaged structs directly and invoke the right TypeProcessor<T> instance
                 * to serialize all other types. */
                if (typeof(T).IsUnmanaged())
                {
                    // writer.Write(enumerator.Current);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadLocal(Locals.Write.IEnumeratorT);
                    il.EmitReadMember(typeof(IEnumerator<T>).GetProperty(nameof(IEnumerator<T>.Current)));
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(T)));
                }
                else
                {
                    // TypeProcessor<T>.Serialize(enumerator.Current, ref writer);
                    il.EmitLoadLocal(Locals.Write.IEnumeratorT);
                    il.EmitReadMember(typeof(IEnumerator<T>).GetProperty(nameof(IEnumerator<T>.Current)));
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(T)));
                }

                // while (enumerator.MoveNext()) { }
                il.MarkLabel(moveNext);
                il.EmitLoadLocal(Locals.Write.IEnumeratorT);
                il.EmitCallvirt(typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext)));
                il.Emit(OpCodes.Brtrue_S, loop);

                // finally { enumerator.Dispose(); }
                il.BeginFinallyBlock();
                il.EmitLoadLocal(Locals.Write.IEnumeratorT);
                il.EmitCallvirt(typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose)));
            }

            // return;
            il.Emit(OpCodes.Ret);
        }

        /// <inheritdoc/>
        protected override void EmitDeserializer(ILGenerator il)
        {
            il.DeclareLocal(typeof(T).MakeArrayType());
            il.DeclareLocals<Locals.Read>();
            il.DeclareLocal(typeof(T).MakeByRefType());

            // int count = reader.Read<int>();
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitCall(KnownMembers.BinaryReader.ReadT(typeof(int)));
            il.Emit(OpCodes.Dup);
            il.EmitStoreLocal(Locals.Read.Count);

            // if (count == -1) return null;
            Label isNotNull = il.DefineLabel();
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Bne_Un_S, isNotNull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(isNotNull);

            // T[] array = new T[count];
            il.EmitLoadLocal(Locals.Read.Count);
            il.Emit(OpCodes.Newarr, typeof(T));
            il.Emit(OpCodes.Dup);
            il.EmitStoreLocal(Locals.Read.ArrayT);

            // ref T r0 = ref array[0];
            il.EmitLoadInt32(0);
            il.Emit(OpCodes.Ldelema, typeof(T));
            il.EmitStoreLocal(Locals.Read.RefT);

            // for (int i = 0; i < count; i++) { }
            Label check = il.DefineLabel();
            il.EmitLoadInt32(0);
            il.EmitStoreLocal(Locals.Read.I);
            il.Emit(OpCodes.Br_S, check);
            Label loop = il.DefineLabel();
            il.MarkLabel(loop);

            // Unsafe.Add(ref r0, i) = TypeProcessor.Deserializer(ref reader);
            il.EmitLoadLocal(Locals.Read.RefT);
            il.EmitLoadLocal(Locals.Read.I);
            il.EmitAddOffset(typeof(T));
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitCall(KnownMembers.TypeProcessor.DeserializerInfo(typeof(T)));
            il.EmitStoreToAddress(typeof(T));

            // i++;
            il.EmitLoadLocal(Locals.Read.I);
            il.EmitLoadInt32(1);
            il.Emit(OpCodes.Add);
            il.EmitStoreLocal(Locals.Read.I);

            // Loop check
            il.MarkLabel(check);
            il.EmitLoadLocal(Locals.Read.I);
            il.EmitLoadLocal(Locals.Read.Count);
            il.Emit(OpCodes.Blt_S, loop);

            // return list;
            il.EmitLoadLocal(Locals.Read.ArrayT);
            il.Emit(OpCodes.Ret);
        }
    }
}

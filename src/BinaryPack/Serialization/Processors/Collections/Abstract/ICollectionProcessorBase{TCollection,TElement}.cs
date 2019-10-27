using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors.Collections.Abstract
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="IEnumerable{T}"/> collection types
    /// </summary>
    /// <typeparam name="TCollection">The type of collection being serialized and deserialized</typeparam>
    /// <typeparam name="TElement">The type of items in the target collection</typeparam>
    internal abstract partial class ICollectionProcessorBase<TCollection, TElement> : TypeProcessor<TCollection?> where TCollection : class, IEnumerable<TElement>
    {
        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            il.DeclareLocal(typeof(IEnumerator<TElement>));

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
            il.EmitReadMember(typeof(TCollection).GetProperty(nameof(ICollection<TElement>.Count))); // Same for IReadOnlyCollection<T>
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
            il.EmitCallvirt(typeof(IEnumerable<TElement>).GetMethod(nameof(IEnumerable<TElement>.GetEnumerator)));
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
                if (typeof(TElement).IsUnmanaged())
                {
                    // writer.Write(enumerator.Current);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadLocal(Locals.Write.IEnumeratorT);
                    il.EmitReadMember(typeof(IEnumerator<TElement>).GetProperty(nameof(IEnumerator<TElement>.Current)));
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(TElement)));
                }
                else
                {
                    // TypeProcessor<T>.Serializer(enumerator.Current, ref writer);
                    il.EmitLoadLocal(Locals.Write.IEnumeratorT);
                    il.EmitReadMember(typeof(IEnumerator<TElement>).GetProperty(nameof(IEnumerator<TElement>.Current)));
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(TElement)));
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
            /* Unlinke with IEnumerable<T> collections, we don't need the
             * additional overhead of a List<T> when dealing with serialized
             * ICollection<T> instances: since we have the total number of items
             * we can just create a T[] array and then assign each item by shifting
             * a ref T variable, which also removes bounds checks in the JITted code. */
            il.DeclareLocal(typeof(TElement).MakeArrayType());
            il.DeclareLocals<Locals.Read>();
            il.DeclareLocal(typeof(TElement).MakeByRefType());

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
            il.Emit(OpCodes.Newarr, typeof(TElement));
            il.EmitStoreLocal(Locals.Read.ArrayT);

            // if (count > 0) { }
            Label end = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.Count);
            il.Emit(OpCodes.Brfalse_S, end);

            // ref T r0 = ref array[0];
            il.EmitLoadLocal(Locals.Read.ArrayT);
            il.EmitLoadInt32(0);
            il.Emit(OpCodes.Ldelema, typeof(TElement));
            il.EmitStoreLocal(Locals.Read.RefT);

            // for (int i = 0; i < count; i++) { }
            Label check = il.DefineLabel();
            il.EmitLoadInt32(0);
            il.EmitStoreLocal(Locals.Read.I);
            il.Emit(OpCodes.Br_S, check);
            Label loop = il.DefineLabel();
            il.MarkLabel(loop);

            // Unsafe.Add(ref r0, i) = reader.Read<T>()/TypeProcessor<T>.Deserializer(ref reader)
            il.EmitLoadLocal(Locals.Read.RefT);
            il.EmitLoadLocal(Locals.Read.I);
            il.EmitAddOffset(typeof(TElement));
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitCall(typeof(TElement).IsUnmanaged()
                ? KnownMembers.BinaryReader.ReadT(typeof(TElement))
                : KnownMembers.TypeProcessor.DeserializerInfo(typeof(TElement)));
            il.EmitStoreToAddress(typeof(TElement));

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

            // return array;
            il.MarkLabel(end);
            il.EmitLoadLocal(Locals.Read.ArrayT);
            il.Emit(OpCodes.Ret);
        }
    }
}

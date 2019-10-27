using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using BinaryPack.Attributes;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors.Collections
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="IDictionary{TKee,TValue}"/> types
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary to serialize and deserialize</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary to serialize and deserialize</typeparam>
    /// <remarks>This processor also works with <see cref="IReadOnlyDictionary{TKey,TValue}"/> instances, as they share the same member accesses</remarks>
    [ProcessorId(1)]
    internal sealed partial class IDictionaryProcessor<TKey, TValue> : TypeProcessor<IDictionary<TKey, TValue>?>
    {
        /// <summary>
        /// Gets the singleton <see cref="IDictionaryProcessor{TKey,TValue}"/> instance to use
        /// </summary>
        public static IDictionaryProcessor<TKey, TValue> Instance { get; } = new IDictionaryProcessor<TKey, TValue>();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            il.DeclareLocal(typeof(IEnumerator<KeyValuePair<TKey, TValue>>));
            il.DeclareLocal(typeof(KeyValuePair<TKey, TValue>));

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
            il.EmitReadMember(typeof(ICollection<KeyValuePair<TKey, TValue>>).GetProperty(nameof(ICollection<TKey>.Count)));
            il.MarkLabel(countLoaded);
            il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(int)));

            // if (object == null) return;
            Label enumeration = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Brtrue_S, enumeration);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(enumeration);

            // using IEnumerator<KeyValuePair<TKey, TValue>> enumerator = obj.GetEnumerator();
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitCallvirt(typeof(IEnumerable<KeyValuePair<TKey, TValue>>).GetMethod(nameof(IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator)));
            il.EmitStoreLocal(Locals.Write.IEnumeratorT);
            using (il.EmitTryBlockScope())
            {
                // Loop start
                Label
                    loop = il.DefineLabel(),
                    moveNext = il.DefineLabel();
                il.Emit(OpCodes.Br_S, moveNext);
                il.MarkLabel(loop);

                // KeyValuePair<TKey, TValue> pair = enumerator.Current;
                il.EmitLoadLocal(Locals.Write.IEnumeratorT);
                il.EmitReadMember(typeof(IEnumerator<KeyValuePair<TKey, TValue>>).GetProperty(nameof(IEnumerator<KeyValuePair<TKey, TValue>>.Current)));
                il.EmitStoreLocal(Locals.Write.KeyValuePairKV);

                // Key serialization
                if (typeof(TKey).IsUnmanaged())
                {
                    // writer.Write(r0.key);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadLocalAddress(Locals.Write.KeyValuePairKV);
                    il.EmitReadMember(typeof(KeyValuePair<TKey, TValue>).GetProperty(nameof(KeyValuePair<TKey, TValue>.Key)));
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(TKey)));
                }
                else
                {
                    // TypeProcessor<TKey>.Serializer(r0.key, ref writer);
                    il.EmitLoadLocalAddress(Locals.Write.KeyValuePairKV);
                    il.EmitReadMember(typeof(KeyValuePair<TKey, TValue>).GetProperty(nameof(KeyValuePair<TKey, TValue>.Key)));
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(TKey)));
                }

                // Value serialization
                if (typeof(TValue).IsUnmanaged())
                {
                    // writer.Write(r0.value);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadLocalAddress(Locals.Write.KeyValuePairKV);
                    il.EmitReadMember(typeof(KeyValuePair<TKey, TValue>).GetProperty(nameof(KeyValuePair<TKey, TValue>.Value)));
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(TValue)));
                }
                else
                {
                    // TypeProcessor<TValue>.Serializer(r0.value, ref writer);
                    il.EmitLoadLocalAddress(Locals.Write.KeyValuePairKV);
                    il.EmitReadMember(typeof(KeyValuePair<TKey, TValue>).GetProperty(nameof(KeyValuePair<TKey, TValue>.Value)));
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(TValue)));
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
            il.DeclareLocal(typeof(Dictionary<TKey, TValue>));
            il.DeclareLocals<Locals.Read>();

            // int count = reader.Read<int>();
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitCall(KnownMembers.BinaryReader.ReadT(typeof(int)));
            il.EmitStoreLocal(Locals.Read.Count);

            // if (count == -1) return null;
            Label isNotNull = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.Count);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Bne_Un_S, isNotNull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);

            // Dictionary<K, V> dictionary = new Dictionary<TKey, TValue>();
            il.MarkLabel(isNotNull);
            il.Emit(OpCodes.Newobj, typeof(Dictionary<TKey, TValue>).GetConstructor(Type.EmptyTypes));
            il.EmitStoreLocal(Locals.Read.DictionaryKV);

            // for (int i = 0; i < count; i++) { }
            Label check = il.DefineLabel();
            il.EmitLoadInt32(0);
            il.EmitStoreLocal(Locals.Read.I);
            il.Emit(OpCodes.Br_S, check);
            Label loop = il.DefineLabel();
            il.MarkLabel(loop);

            // dictionary(...);
            il.EmitLoadLocal(Locals.Read.DictionaryKV);

            // TKey key = reader.Read<TKey>()/TypeProcessor<TKey>.Deserializer(ref reader);
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitCall(typeof(TKey).IsUnmanaged()
                ? KnownMembers.BinaryReader.ReadT(typeof(TKey))
                : KnownMembers.TypeProcessor.DeserializerInfo(typeof(TKey)));

            // TValue value = reader.Read<TValue>()/TypeProcessor<TValue>.Deserializer(ref reader);
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitCall(typeof(TValue).IsUnmanaged()
                ? KnownMembers.BinaryReader.ReadT(typeof(TValue))
                : KnownMembers.TypeProcessor.DeserializerInfo(typeof(TValue)));

            // dictionary.Add(key, value);
            il.EmitCallvirt(typeof(Dictionary<TKey, TValue>).GetMethod(nameof(Dictionary<TKey, TValue>.Add)));

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

            // return dictionary;
            il.EmitLoadLocal(Locals.Read.DictionaryKV);
            il.Emit(OpCodes.Ret);
        }
    }
}

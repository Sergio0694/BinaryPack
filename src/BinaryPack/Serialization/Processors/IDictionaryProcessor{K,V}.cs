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
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="IDictionary{K,V}"/> types
    /// </summary>
    /// <typeparam name="K">The type of the keys in the dictionary to serialize and deserialize</typeparam>
    /// <typeparam name="V">The type of the values in the dictionary to serialize and deserialize</typeparam>
    [ProcessorId(5)]
    internal sealed partial class IDictionaryProcessor<K, V> : TypeProcessor<IDictionary<K, V>?>
    {
        /// <summary>
        /// Gets the singleton <see cref="IDictionaryProcessor{K,V}"/> instance to use
        /// </summary>
        public static IDictionaryProcessor<K, V> Instance { get; } = new IDictionaryProcessor<K, V>();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            il.DeclareLocal(typeof(IEnumerator<KeyValuePair<K, V>>));
            il.DeclareLocal(typeof(KeyValuePair<K, V>));

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
            il.EmitReadMember(typeof(ICollection<KeyValuePair<K, V>>).GetProperty(nameof(ICollection<K>.Count)));
            il.MarkLabel(countLoaded);
            il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(int)));

            // if (object == null) return;
            Label enumeration = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Brtrue_S, enumeration);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(enumeration);

            // using IEnumerator<KeyValuePair<K, V>> enumerator = obj.GetEnumerator();
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitCallvirt(typeof(IEnumerable<KeyValuePair<K, V>>).GetMethod(nameof(IEnumerable<KeyValuePair<K, V>>.GetEnumerator)));
            il.EmitStoreLocal(Locals.Write.IEnumeratorT);
            using (il.EmitTryBlockScope())
            {
                // Loop start
                Label
                    loop = il.DefineLabel(),
                    moveNext = il.DefineLabel();
                il.Emit(OpCodes.Br_S, moveNext);
                il.MarkLabel(loop);

                // KeyValuePair<K, V> pair = enumerator.Current;
                il.EmitLoadLocal(Locals.Write.IEnumeratorT);
                il.EmitReadMember(typeof(IEnumerator<KeyValuePair<K, V>>).GetProperty(nameof(IEnumerator<KeyValuePair<K, V>>.Current)));
                il.EmitStoreLocal(Locals.Write.KeyValuePairKV);

                // Key serialization
                if (typeof(K).IsUnmanaged())
                {
                    // writer.Write(r0.key);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadLocalAddress(Locals.Write.KeyValuePairKV);
                    il.EmitReadMember(typeof(KeyValuePair<K, V>).GetProperty(nameof(KeyValuePair<K, V>.Key)));
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(K)));
                }
                else
                {
                    // TypeProcessor<T>.Serializer(r0.key, ref writer);
                    il.EmitLoadLocalAddress(Locals.Write.KeyValuePairKV);
                    il.EmitReadMember(typeof(KeyValuePair<K, V>).GetProperty(nameof(KeyValuePair<K, V>.Key)));
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(K)));
                }

                // Value serialization
                if (typeof(V).IsUnmanaged())
                {
                    // writer.Write(r0.value);
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitLoadLocalAddress(Locals.Write.KeyValuePairKV);
                    il.EmitReadMember(typeof(KeyValuePair<K, V>).GetProperty(nameof(KeyValuePair<K, V>.Value)));
                    il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(V)));
                }
                else
                {
                    // TypeProcessor<T>.Serializer(r0.value, ref writer);
                    il.EmitLoadLocalAddress(Locals.Write.KeyValuePairKV);
                    il.EmitReadMember(typeof(KeyValuePair<K, V>).GetProperty(nameof(KeyValuePair<K, V>.Value)));
                    il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                    il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(V)));
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
            il.DeclareLocal(typeof(Dictionary<K, V>));
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

            // Dictionary<K, V> dictionary = new Dictionary<K, V>();
            il.MarkLabel(isNotNull);
            il.Emit(OpCodes.Newobj, typeof(Dictionary<K, V>).GetConstructor(Type.EmptyTypes));
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

            // K key = reader.Read<K>()/TypeProcessor<K>.Deserializer(ref reader);
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitCall(typeof(K).IsUnmanaged()
                ? KnownMembers.BinaryReader.ReadT(typeof(K))
                : KnownMembers.TypeProcessor.DeserializerInfo(typeof(K)));

            // V value = reader.Read<V>()/TypeProcessor<V>.Deserializer(ref reader);
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitCall(typeof(V).IsUnmanaged()
                ? KnownMembers.BinaryReader.ReadT(typeof(V))
                : KnownMembers.TypeProcessor.DeserializerInfo(typeof(V)));

            // dictionary.Add(key, value);
            il.EmitCallvirt(typeof(Dictionary<K, V>).GetMethod(nameof(Dictionary<K, V>.Add)));

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

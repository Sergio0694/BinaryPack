using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Attributes;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors.Collections
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="Dictionary{K,V}"/> types
    /// </summary>
    /// <typeparam name="K">The type of the keys in the dictionary to serialize and deserialize</typeparam>
    /// <typeparam name="V">The type of the values in the dictionary to serialize and deserialize</typeparam>
    [ProcessorId(0)]
    internal sealed partial class DictionaryProcessor<K, V> : TypeProcessor<Dictionary<K, V>?>
    {
        /// <summary>
        /// The <see cref="Type"/> instance for the nested <see cref="Dictionary{TKey,TValue}"/>.Entry <see langword="struct"/>
        /// </summary>
        private static readonly Type EntryType = typeof(Dictionary<K, V>).GetGenericNestedType("Entry");

        /// <summary>
        /// Gets the singleton <see cref="DictionaryProcessor{K,V}"/> instance to use
        /// </summary>
        public static DictionaryProcessor<K, V> Instance { get; } = new DictionaryProcessor<K, V>();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            il.DeclareLocals<Locals.Write>();
            il.DeclareLocal(EntryType.MakeByRefType());

            //int count = obj?.Count ?? - 1;
            Label
                isNotNull = il.DefineLabel(),
                countLoaded = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Brtrue_S, isNotNull);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Br_S, countLoaded);
            il.MarkLabel(isNotNull);
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitReadMember(typeof(Dictionary<K, V>).GetProperty(nameof(Dictionary<K, V>.Count)));
            il.MarkLabel(countLoaded);
            il.EmitStoreLocal(Locals.Write.Count);

            // writer.Write(count);
            il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
            il.EmitLoadLocal(Locals.Write.Count);
            il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(int)));

            // if (count > 0) { }
            Label end = il.DefineLabel();
            il.EmitLoadLocal(Locals.Write.Count);
            il.EmitLoadInt32(0);
            il.Emit(OpCodes.Ble_S, end);

            // ref Entry r0 = ref obj._entries[0];
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitReadMember(typeof(Dictionary<K, V>).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance));
            il.EmitLoadInt32(0);
            il.Emit(OpCodes.Ldelema, EntryType);
            il.EmitStoreLocal(Locals.Write.RefEntry);

            // for (int i = 0; i < count;;) { }
            Label check = il.DefineLabel();
            il.EmitLoadInt32(0);
            il.EmitStoreLocal(Locals.Write.I);
            il.Emit(OpCodes.Br_S, check);
            Label loop = il.DefineLabel();
            il.MarkLabel(loop);

            // if (r0.next >= -1) { }
            Label emptyEntry = il.DefineLabel();
            il.EmitLoadLocal(Locals.Write.RefEntry);
            il.EmitReadMember(EntryType.GetField("next"));
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Blt_S, emptyEntry);

            // Key serialization
            if (typeof(K).IsUnmanaged())
            {
                // writer.Write(r0.key);
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitLoadLocal(Locals.Write.RefEntry);
                il.EmitReadMember(EntryType.GetField("key"));
                il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(K)));
            }
            else
            {
                // TypeProcessor<T>.Serializer(r0.key, ref writer);
                il.EmitLoadLocal(Locals.Write.RefEntry);
                il.EmitReadMember(EntryType.GetField("key"));
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(K)));
            }

            // Value serialization
            if (typeof(V).IsUnmanaged())
            {
                // writer.Write(r0.value);
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitLoadLocal(Locals.Write.RefEntry);
                il.EmitReadMember(EntryType.GetField("value"));
                il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(V)));
            }
            else
            {
                // TypeProcessor<T>.Serializer(r0.value, ref writer);
                il.EmitLoadLocal(Locals.Write.RefEntry);
                il.EmitReadMember(EntryType.GetField("value"));
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(V)));
            }

            // i++
            il.EmitLoadLocal(Locals.Write.I);
            il.EmitLoadInt32(1);
            il.Emit(OpCodes.Add);
            il.EmitStoreLocal(Locals.Write.I);
            il.MarkLabel(emptyEntry);

            // r0 = ref Unsafe.Add(ref r0, 1);
            il.EmitLoadLocal(Locals.Write.RefEntry);
            il.Emit(OpCodes.Sizeof, EntryType);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Add);
            il.EmitStoreLocal(Locals.Write.RefEntry);

            // Loop check
            il.MarkLabel(check);
            il.EmitLoadLocal(Locals.Write.I);
            il.EmitLoadLocal(Locals.Write.Count);
            il.Emit(OpCodes.Blt_S, loop);

            // return;
            il.MarkLabel(end);
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

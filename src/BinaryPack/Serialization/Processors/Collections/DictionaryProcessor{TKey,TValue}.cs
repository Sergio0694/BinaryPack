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
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="Dictionary{TKey,TValue}"/> types
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary to serialize and deserialize</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary to serialize and deserialize</typeparam>
    [ProcessorId(0)]
    internal sealed partial class DictionaryProcessor<TKey, TValue> : TypeProcessor<Dictionary<TKey, TValue>?>
    {
        /// <summary>
        /// The <see cref="Type"/> instance for the nested <see cref="Dictionary{TKey,TValue}"/>.Entry <see langword="struct"/>
        /// </summary>
        private static readonly Type EntryType = typeof(Dictionary<TKey, TValue>).GetGenericNestedType("Entry");

        /// <summary>
        /// Gets the singleton <see cref="DictionaryProcessor{TKey,TValue}"/> instance to use
        /// </summary>
        public static DictionaryProcessor<TKey, TValue> Instance { get; } = new DictionaryProcessor<TKey, TValue>();

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
            il.EmitReadMember(typeof(Dictionary<TKey, TValue>).GetProperty(nameof(Dictionary<TKey, TValue>.Count)));
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
            il.EmitReadMember(typeof(Dictionary<TKey, TValue>).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance));
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
            if (typeof(TKey).IsUnmanaged())
            {
                // writer.Write(r0.key);
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitLoadLocal(Locals.Write.RefEntry);
                il.EmitReadMember(EntryType.GetField("key"));
                il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(TKey)));
            }
            else
            {
                // TypeProcessor<T>.Serializer(r0.key, ref writer);
                il.EmitLoadLocal(Locals.Write.RefEntry);
                il.EmitReadMember(EntryType.GetField("key"));
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(TKey)));
            }

            // Value serialization
            if (typeof(TValue).IsUnmanaged())
            {
                // writer.Write(r0.value);
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitLoadLocal(Locals.Write.RefEntry);
                il.EmitReadMember(EntryType.GetField("value"));
                il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(TValue)));
            }
            else
            {
                // TypeProcessor<T>.Serializer(r0.value, ref writer);
                il.EmitLoadLocal(Locals.Write.RefEntry);
                il.EmitReadMember(EntryType.GetField("value"));
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(TValue)));
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

            // Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
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

            // K key = reader.Read<TKey>()/TypeProcessor<TKey>.Deserializer(ref reader);
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitCall(typeof(TKey).IsUnmanaged()
                ? KnownMembers.BinaryReader.ReadT(typeof(TKey))
                : KnownMembers.TypeProcessor.DeserializerInfo(typeof(TKey)));

            // V value = reader.Read<TValue>()/TypeProcessor<TValue>.Deserializer(ref reader);
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

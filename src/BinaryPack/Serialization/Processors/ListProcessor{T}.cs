using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Extensions;
using BinaryPack.Extensions.System.Reflection.Emit;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Extensions;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="List{T}"/> types
    /// </summary>
    /// <typeparam name="T">The type of items in arrays to serialize and deserialize</typeparam>
    internal sealed partial class ListProcessor<T> : TypeProcessor<List<T>?>
    {
        /// <summary>
        /// Gets the singleton <see cref="ArrayProcessor{T}"/> instance to use
        /// </summary>
        public static ListProcessor<T> Instance { get; } = new ListProcessor<T>();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            // ReadOnlySpan<T> span; ...;
            il.DeclareLocal(typeof(ReadOnlySpan<T>));
            il.DeclareLocals<Locals.Write>();

            // int count = obj?.Count ?? -1;
            Label
                notNull = il.DefineLabel(),
                countLoaded = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Brtrue_S, notNull);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Br_S, countLoaded);
            il.MarkLabel(notNull);
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitReadMember(typeof(List<T>).GetField("_size", BindingFlags.NonPublic | BindingFlags.Instance));
            il.MarkLabel(countLoaded);
            il.EmitStoreLocal(Locals.Write.Count);

            // byte* p = stackalloc byte[sizeof(int)]; *(int*)p = length;
            il.EmitStackalloc(typeof(int));
            il.EmitStoreLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.Count);
            il.EmitStoreToAddress(typeof(int));

            // stream.Write(new ReadOnlySpan<byte>(p, 4));
            il.EmitLoadArgument(Arguments.Write.Stream);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadInt32(sizeof(int));
            il.Emit(OpCodes.Newobj, KnownMembers.ReadOnlySpan.UnsafeConstructor(typeof(byte)));
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Write, null);

            // ReadOnlySpan<T> span = new ReadOnlySpan<T>(obj._items, 0, count);
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitReadMember(typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance));
            il.EmitLoadInt32(0);
            il.EmitLoadLocal(Locals.Write.Count);
            il.Emit(OpCodes.Newobj, KnownMembers.ReadOnlySpan.ArrayWithOffsetAndLengthConstructor(typeof(T)));
            il.EmitStoreLocal(Locals.Write.ReadOnlySpanT);

            /* Just like in ArrayProcessor<T>, handle unmanaged types as a special case.
             * If T is unmanaged, the whole buffer is written directly to the stream
             * after being broadcast as a byte span. If T is a string, the dedicated
             * serializer is invoked. For all other cases, the standard object serializer is used. */
            if (typeof(T).IsUnmanaged())
            {
                // if (size <= 0) return;
                Label copy = il.DefineLabel();
                il.EmitLoadLocal(Locals.Write.Count);
                il.EmitLoadInt32(0);
                il.Emit(OpCodes.Bge_S, copy);
                il.Emit(OpCodes.Ret);

                // stream.Write(MemoryMarshal.AsBytes(span));
                il.MarkLabel(copy);
                il.EmitLoadArgument(Arguments.Write.Stream);
                il.EmitLoadLocal(Locals.Write.ReadOnlySpanT);
                il.EmitCall(OpCodes.Call, KnownMembers.MemoryMarshal.AsByteReadOnlySpan(typeof(T)), null);
                il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Write, null);
                il.Emit(OpCodes.Ret);
            }
            else
            {
                // for (int i = 0; i < count; i++) { }
                Label check = il.DefineLabel();
                il.EmitLoadInt32(0);
                il.EmitStoreLocal(Locals.Write.I);
                il.Emit(OpCodes.Br_S, check);
                Label loop = il.DefineLabel();
                il.MarkLabel(loop);

                // ...(span[i], stream);
                il.EmitLoadLocalAddress(Locals.Write.ReadOnlySpanT);
                il.EmitLoadLocal(Locals.Write.I);
                il.EmitCall(OpCodes.Call, KnownMembers.ReadOnlySpan.GetterAt(typeof(T)), null);
                il.Emit(typeof(T).IsValueType ? OpCodes.Ldobj : OpCodes.Ldelem_Ref);
                il.EmitLoadArgument(Arguments.Write.Stream);

                // StringProcessor/ObjectProcessor<T>.Serialize(...);
                MethodInfo methodInfo = typeof(T) == typeof(string)
                    ? StringProcessor.Instance.SerializerInfo.MethodInfo
                    : KnownMembers.ObjectProcessor.SerializerInfo(typeof(T));
                il.EmitCall(OpCodes.Call, methodInfo, null);

                // i++;
                il.EmitLoadLocal(Locals.Write.I);
                il.EmitLoadInt32(1);
                il.Emit(OpCodes.Add);
                il.EmitStoreLocal(Locals.Write.I);

                // Loop check
                il.MarkLabel(check);
                il.EmitLoadLocal(Locals.Write.I);
                il.EmitLoadLocal(Locals.Write.Count);
                il.Emit(OpCodes.Blt_S, loop);
                il.Emit(OpCodes.Ret);
            }
        }

        /// <inheritdoc/>
        protected override void EmitDeserializer(ILGenerator il)
        {
            // List<T> list; ...;
            il.DeclareLocal(typeof(List<T>));
            il.DeclareLocal(typeof(T[]));
            il.DeclareLocals<Locals.Read>();

            // Span<byte> span = stackalloc byte[sizeof(int)];
            il.EmitStackalloc(typeof(int));
            il.EmitLoadInt32(sizeof(int));
            il.Emit(OpCodes.Newobj, KnownMembers.Span.UnsafeConstructor(typeof(byte)));
            il.EmitStoreLocal(Locals.Read.SpanByte);

            // _ = stream.Read(span);
            il.EmitLoadArgument(Arguments.Read.Stream);
            il.EmitLoadLocal(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Read, null);
            il.Emit(OpCodes.Pop);

            // int count = span.GetPinnableReference();
            il.EmitLoadLocalAddress(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Call, KnownMembers.Span.GetPinnableReference(typeof(byte)), null);
            il.EmitLoadFromAddress(typeof(int));
            il.EmitStoreLocal(Locals.Read.Count);

            // if (count == -1) return null;
            Label isNotNull = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.Count);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Bne_Un_S, isNotNull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);

            // if (count == 0) return new List<T>();
            Label isNotEmpty = il.DefineLabel();
            il.MarkLabel(isNotNull);
            il.Emit(OpCodes.Newobj, typeof(List<T>).GetConstructor(Type.EmptyTypes));
            il.EmitLoadLocal(Locals.Read.Count);
            il.Emit(OpCodes.Brtrue_S, isNotEmpty);
            il.Emit(OpCodes.Ret);

            // else list = new List<T>();
            il.MarkLabel(isNotEmpty);

            // list._size = count;
            il.EmitLoadLocal(Locals.Read.ListT);
            il.EmitLoadLocal(Locals.Read.Count);
            il.EmitWriteMember(typeof(List<T>).GetField("_size", BindingFlags.NonPublic | BindingFlags.Instance));

            // Local function to calculate the 2^x size of the target T[] array
            static int CalculateCapacityFromCount(int n)
            {
                int log = 0;

                if (n > 0xFFFF) { n >>= 16; log = 16; }
                if (n > 0xff) { n >>= 8; log |= 8; }
                if (n > 0xf) { n >>= 4; log |= 4; }
                if (n > 0x3) { n >>= 2; log |= 2; }
                if (n > 0x1) { log |= 1; }

                return 1 << (log + 1);
            }

            // T[] array = new T[CalculateCapacityFromCount(count)];
            il.EmitLoadLocal(Locals.Read.Count);
            il.EmitCall(OpCodes.Call, typeof(ListProcessor<T>).GetMethod(nameof(CalculateCapacityFromCount), BindingFlags.NonPublic | BindingFlags.Static), null);
            il.Emit(OpCodes.Newarr, typeof(T));
            il.EmitStoreLocal(Locals.Read.ArrayT);

            if (typeof(T).IsUnmanaged())
            {
                // _ = stream.Read(MemoryMarshal.AsBytes(new Span<T>(array, 0, count)));
                il.EmitLoadArgument(Arguments.Read.Stream);
                il.EmitLoadLocal(Locals.Read.ArrayT);
                il.EmitLoadInt32(0);
                il.EmitLoadLocal(Locals.Read.Count);
                il.Emit(OpCodes.Newobj, KnownMembers.Span.ArrayWithOffsetAndLengthConstructor(typeof(T)));
                il.EmitCall(OpCodes.Call, KnownMembers.MemoryMarshal.AsByteSpan(typeof(T)), null);
                il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Read, null);
                il.Emit(OpCodes.Pop);
            }
            else
            {
                // for (int i = 0; i < count; i++) { }
                Label check = il.DefineLabel();
                il.EmitLoadInt32(0);
                il.EmitStoreLocal(Locals.Read.I);
                il.Emit(OpCodes.Br_S, check);
                Label loop = il.DefineLabel();
                il.MarkLabel(loop);

                // StringProcessor/ObjectProcessor<T>.Deserialize
                MethodInfo methodInfo = typeof(T) == typeof(string)
                    ? StringProcessor.Instance.DeserializerInfo.MethodInfo
                    : KnownMembers.ObjectProcessor.DeserializerInfo(typeof(T));

                // array[i] = ...(stream);
                il.EmitLoadLocal(Locals.Read.ArrayT);
                il.EmitLoadLocal(Locals.Read.I);
                il.EmitLoadArgument(Arguments.Read.Stream);
                il.EmitCall(OpCodes.Call, methodInfo, null);
                il.Emit(OpCodes.Stelem_Ref);

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
            }

            // list._items = array;
            il.EmitLoadLocal(Locals.Read.ListT);
            il.EmitLoadLocal(Locals.Read.ArrayT);
            il.EmitWriteMember(typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance));

            // return list;
            il.EmitLoadLocal(Locals.Read.ListT);
            il.Emit(OpCodes.Ret);
        }
    }
}


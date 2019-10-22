using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Serialization.Constants;
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
            /* Just like in ArrayProcessor<T>, declare the shared variables,
             * and the additional ref T variable if the T is not an unmanaged type. */
            il.DeclareLocals<Locals.Write>();
            if (!typeof(T).IsUnmanaged()) il.DeclareLocal(typeof(T).MakeByRefType());

            // int count = obj?._size ?? -1;
            Label
                isNotNull = il.DefineLabel(),
                countLoaded = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Brtrue_S, isNotNull);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Br_S, countLoaded);
            il.MarkLabel(isNotNull);
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitReadMember(typeof(List<T>).GetField("_size", BindingFlags.NonPublic | BindingFlags.Instance));
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

            /* Just like in ArrayProcessor<T>, handle unmanaged types as a special case.
             * If T is unmanaged, the whole buffer is written directly to the stream
             * after being broadcast as a byte span. If T is a string, the dedicated
             * serializer is invoked. For all other cases, the standard object serializer is used. */
            if (typeof(T).IsUnmanaged())
            {
                // writer.Write(obj._items.AsSpan(0, count));
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitLoadArgument(Arguments.Write.T);
                il.EmitReadMember(typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance));
                il.EmitLoadInt32(0);
                il.EmitLoadLocal(Locals.Write.Count);
                il.Emit(OpCodes.Newobj, KnownMembers.Span.ArrayWithOffsetAndLengthConstructor(typeof(T)));
                il.EmitCall(KnownMembers.BinaryWriter.WriteSpanT(typeof(T)));
            }
            else
            {
                // ref T r0 = ref obj._items[0];
                il.EmitLoadArgument(Arguments.Write.T);
                il.EmitReadMember(typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance));
                il.EmitLoadInt32(0);
                il.Emit(OpCodes.Ldelema, typeof(T));
                il.EmitStoreLocal(Locals.Write.RefT);

                // for (int i = 0; i < count; i++) { }
                Label check = il.DefineLabel();
                il.EmitLoadInt32(0);
                il.EmitStoreLocal(Locals.Write.I);
                il.Emit(OpCodes.Br_S, check);
                Label loop = il.DefineLabel();
                il.MarkLabel(loop);

                // ...(Unsafe.Add(ref r0, i), ref writer);
                il.EmitLoadLocal(Locals.Write.RefT);
                il.EmitLoadLocal(Locals.Write.I);
                il.EmitAddOffset(typeof(T));
                il.EmitLoadFromAddress(typeof(T));
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);

                // StringProcessor/ObjectProcessor<T>.Serialize(...);
                MethodInfo methodInfo = typeof(T) == typeof(string)
                    ? StringProcessor.Instance.SerializerInfo.MethodInfo
                    : KnownMembers.TypeProcessor.SerializerInfo(typeof(ObjectProcessor<>), typeof(T));
                il.EmitCall(methodInfo);

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
            }

            // return;
            il.MarkLabel(end);
            il.Emit(OpCodes.Ret);
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
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitLoadLocal(Locals.Read.SpanByte);
            il.EmitCallvirt(KnownMembers.Stream.Read);
            il.Emit(OpCodes.Pop);

            // int count = span.GetPinnableReference();
            il.EmitLoadLocalAddress(Locals.Read.SpanByte);
            il.EmitCall(KnownMembers.Span.GetPinnableReference(typeof(byte)));
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
            il.EmitStoreLocal(Locals.Read.ListT);

            // list._size = count;
            il.EmitLoadLocal(Locals.Read.ListT);
            il.EmitLoadLocal(Locals.Read.Count);
            il.EmitWriteMember(typeof(List<T>).GetField("_size", BindingFlags.NonPublic | BindingFlags.Instance));

            // T[] array = new T[CalculateCapacityFromCount(count)];
            il.EmitLoadLocal(Locals.Read.Count);
            il.EmitCall(typeof(NumericExtensions).GetMethod(nameof(NumericExtensions.UpperBoundLog2)));
            il.Emit(OpCodes.Newarr, typeof(T));
            il.EmitStoreLocal(Locals.Read.ArrayT);

            if (typeof(T).IsUnmanaged())
            {
                // _ = stream.Read(MemoryMarshal.AsBytes(new Span<T>(array, 0, count)));
                il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                il.EmitLoadLocal(Locals.Read.ArrayT);
                il.EmitLoadInt32(0);
                il.EmitLoadLocal(Locals.Read.Count);
                il.Emit(OpCodes.Newobj, KnownMembers.Span.ArrayWithOffsetAndLengthConstructor(typeof(T)));
                il.EmitCall(KnownMembers.MemoryMarshal.AsByteSpan(typeof(T)));
                il.EmitCallvirt(KnownMembers.Stream.Read);
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
                    : KnownMembers.TypeProcessor.DeserializerInfo(typeof(ObjectProcessor<>), typeof(T));

                // array[i] = ...(stream);
                il.EmitLoadLocal(Locals.Read.ArrayT);
                il.EmitLoadLocal(Locals.Read.I);
                il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                il.EmitCall(methodInfo);
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


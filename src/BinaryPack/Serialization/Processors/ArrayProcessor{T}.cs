using System;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for array types
    /// </summary>
    /// <typeparam name="T">The type of items in arrays to serialize and deserialize</typeparam>
    internal sealed partial class ArrayProcessor<T> : TypeProcessor<T[]?>
    {
        /// <summary>
        /// Gets the singleton <see cref="ArrayProcessor{T}"/> instance to use
        /// </summary>
        public static ArrayProcessor<T> Instance { get; } = new ArrayProcessor<T>();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            /* Declare the local variables that are shared across all the
             * different implementations, and the additional ref T variable if
             * T is not an unmanaged type. This is a micro-optimization to speed up
             * the loop iterations when the whole array can't be copied directly. */
            il.DeclareLocals<Locals.Write>();
            if (!typeof(T).IsUnmanaged()) il.DeclareLocal(typeof(T).MakeByRefType());

            // int length = obj?.Length ?? -1;
            Label
                isNotNull = il.DefineLabel(),
                lengthLoaded = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Brtrue_S, isNotNull);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Br_S, lengthLoaded);
            il.MarkLabel(isNotNull);
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.MarkLabel(lengthLoaded);
            il.EmitStoreLocal(Locals.Write.Length);

            // writer.Write(length);
            il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
            il.EmitLoadLocal(Locals.Write.Length);
            il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(int)));

            // if (size > 0) { }
            Label end = il.DefineLabel();
            il.EmitLoadLocal(Locals.Write.Length);
            il.EmitLoadInt32(0);
            il.Emit(OpCodes.Ble_S, end);

            /* The generic type parameter T doesn't have constraints, and there are three
             * main cases that need to be handled. This is all done while building the
             * method, so there are no actual checks being performed during serialization.
             * If T is unmanaged, the whole array is written directly to the stream.
             * If T is a string, the dedicated serializer is invoked. For all other
             * cases,the standard object serializer is used. */
            if (typeof(T).IsUnmanaged())
            {
                // writer.Write(obj.AsSpan());
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitLoadArgument(Arguments.Write.T);
                il.Emit(OpCodes.Newobj, KnownMembers.Span.ArrayConstructor(typeof(T)));
                il.EmitCall(KnownMembers.BinaryWriter.WriteSpanT(typeof(T)));
            }
            else
            {
                // ref T r0 = ref obj[0];
                il.EmitLoadArgument(Arguments.Write.T);
                il.EmitLoadInt32(0);
                il.Emit(OpCodes.Ldelema, typeof(T));
                il.EmitStoreLocal(Locals.Write.RefT);

                // for (int i = 0; i < length; i++) { }
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
                il.EmitLoadLocal(Locals.Write.Length);
                il.Emit(OpCodes.Blt_S, loop);
            }

            // return;
            il.MarkLabel(end);
            il.Emit(OpCodes.Ret);
        }

        /// <inheritdoc/>
        protected override void EmitDeserializer(ILGenerator il)
        {
            // T[] array; ...;
            il.DeclareLocal(typeof(T).MakeArrayType());
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

            // int length = span.GetPinnableReference();
            il.EmitLoadLocalAddress(Locals.Read.SpanByte);
            il.EmitCall(KnownMembers.Span.GetPinnableReference(typeof(byte)));
            il.EmitLoadFromAddress(typeof(int));
            il.EmitStoreLocal(Locals.Read.Length);

            // if (length == -1) return null;
            Label isNotNull = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.Length);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Bne_Un_S, isNotNull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);

            // if (length == 0) return Array.Empty<T>();
            Label isNotEmpty = il.DefineLabel();
            il.MarkLabel(isNotNull);
            il.EmitLoadLocal(Locals.Read.Length);
            il.Emit(OpCodes.Brtrue_S, isNotEmpty);
            il.EmitCall(typeof(Array).GetMethod(nameof(Array.Empty)).MakeGenericMethod(typeof(T)));
            il.Emit(OpCodes.Ret);

            // else array = new T[length];
            il.MarkLabel(isNotEmpty);
            il.EmitLoadLocal(Locals.Read.Length);
            il.Emit(OpCodes.Newarr, typeof(T));
            il.EmitStoreLocal(Locals.Read.ArrayT);

            if (typeof(T).IsUnmanaged())
            {
                // _ = stream.Read(MemoryMarshal.AsBytes(new Span<T>(array)));
                il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                il.EmitLoadLocal(Locals.Read.ArrayT);
                il.Emit(OpCodes.Newobj, KnownMembers.Span.ArrayConstructor(typeof(T)));
                il.EmitCall(KnownMembers.MemoryMarshal.AsByteSpan(typeof(T)));
                il.EmitCallvirt(KnownMembers.Stream.Read);
                il.Emit(OpCodes.Pop);
            }
            else
            {
                // for (int i = 0; i < length; i++) { }
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
                il.EmitLoadLocal(Locals.Read.Length);
                il.Emit(OpCodes.Blt_S, loop);
            }

            // return array;
            il.EmitLoadLocal(Locals.Read.ArrayT);
            il.Emit(OpCodes.Ret);
        }
    }
}

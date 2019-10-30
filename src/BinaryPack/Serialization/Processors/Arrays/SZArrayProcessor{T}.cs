using System;
using System.Reflection.Emit;
using BinaryPack.Attributes;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors.Arrays
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for 1D array types
    /// </summary>
    /// <typeparam name="T">The type of items in arrays to serialize and deserialize</typeparam>
    [ProcessorId(1)]
    internal sealed partial class SZArrayProcessor<T> : TypeProcessor<T[]?>
    {
        /// <summary>
        /// Gets the singleton <see cref="SZArrayProcessor{T}"/> instance to use
        /// </summary>
        public static SZArrayProcessor<T> Instance { get; } = new SZArrayProcessor<T>();

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

            // if (length > 0) { }
            Label end = il.DefineLabel();
            il.EmitLoadLocal(Locals.Write.Length);
            il.EmitLoadInt32(0);
            il.Emit(OpCodes.Ble_S, end);

            /* The generic type parameter T doesn't have constraints, and there are two
             * cases that need to be handled. This is all done while building the method,
             * so there are no actual checks being performed during serialization.
             * If T is unmanaged, the whole array is written directly to the stream.
             * Otherwise, the right object serializer is used depending on the current type. */
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

                // TypeProcessor<T>.Serializer(Unsafe.Add(ref r0, i), ref writer);
                il.EmitLoadLocal(Locals.Write.RefT);
                il.EmitLoadLocal(Locals.Write.I);
                il.EmitAddOffset(typeof(T));
                il.EmitLoadFromAddress(typeof(T));
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(T)));

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
            /* Just like the serialization method, declare the shared local
             * variables and then the optional ref T variable for arrays
             * where T doesn't respect the unmanaged type constraint. */
            il.DeclareLocal(typeof(T).MakeArrayType());
            il.DeclareLocals<Locals.Read>();
            if (!typeof(T).IsUnmanaged()) il.DeclareLocal(typeof(T).MakeByRefType());

            // int length = reader.Read<int>();
            il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
            il.EmitCall(KnownMembers.BinaryReader.ReadT(typeof(int)));
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

            // T[] array = new T[length];
            il.MarkLabel(isNotEmpty);
            il.EmitLoadLocal(Locals.Read.Length);
            il.Emit(OpCodes.Newarr, typeof(T));
            il.EmitStoreLocal(Locals.Read.ArrayT);

            if (typeof(T).IsUnmanaged())
            {
                // reader.Read(new Span<T>(array));
                il.EmitLoadArgument(Arguments.Read.RefBinaryReader);
                il.EmitLoadLocal(Locals.Read.ArrayT);
                il.Emit(OpCodes.Newobj, KnownMembers.Span.ArrayConstructor(typeof(T)));
                il.EmitCall(KnownMembers.BinaryReader.ReadSpanT(typeof(T)));
            }
            else
            {
                // ref T r0 = ref array[0];
                il.EmitLoadLocal(Locals.Read.ArrayT);
                il.EmitLoadInt32(0);
                il.Emit(OpCodes.Ldelema, typeof(T));
                il.EmitStoreLocal(Locals.Read.RefT);

                // for (int i = 0; i < length; i++) { }
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
                il.EmitLoadLocal(Locals.Read.Length);
                il.Emit(OpCodes.Blt_S, loop);
            }

            // return array;
            il.EmitLoadLocal(Locals.Read.ArrayT);
            il.Emit(OpCodes.Ret);
        }
    }
}

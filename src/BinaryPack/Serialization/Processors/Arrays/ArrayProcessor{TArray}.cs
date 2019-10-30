using System;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors.Arrays
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for ND array types
    /// </summary>
    /// <typeparam name="TArray">The type of array to serialize and deserialize</typeparam>
    internal sealed partial class ArrayProcessor<TArray> : TypeProcessor<TArray?> where TArray : class
    {
        /// <summary>
        /// Static <see cref="ArrayProcessor{TArray}"/> constructor to programmatically validate <typeparamref name="TArray"/>
        /// </summary>
        static ArrayProcessor()
        {
            if (typeof(TArray).IsArray &&
                typeof(TArray).GetArrayRank() > 1 &&
                !typeof(TArray).IsVariableBoundArray) return;

            throw new ArgumentException($"{nameof(ArrayProcessor<TArray>)} only works on ND, 0-index arrays, not on [{typeof(TArray)}]");
        }

        /// <summary>
        /// The type of items in the arrays to serialize and deserialize
        /// </summary>
        private static readonly Type T = typeof(TArray).GetElementType();

        /// <summary>
        /// The rank of arrays to serialize and deserialize
        /// </summary>
        private static readonly int Rank = typeof(TArray).GetArrayRank();

        /// <summary>
        /// Gets the singleton <see cref="ArrayProcessor{TArray}"/> instance to use
        /// </summary>
        public static ArrayProcessor<TArray> Instance { get; } = new ArrayProcessor<TArray>();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            /* As in the SZArrayProcessor<T>, declare the shared local variables
             * and the ref T variable for the inner loop if T is not an unmanaged type. */
            il.DeclareLocals<Locals.Write>();
            if (!T.IsUnmanaged()) il.DeclareLocal(T.MakeByRefType());

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
            il.EmitReadMember(typeof(Array).GetProperty(nameof(Array.Length)));
            il.MarkLabel(lengthLoaded);
            il.EmitStoreLocal(Locals.Write.Length);

            // writer.Write(length);
            il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
            il.EmitLoadLocal(Locals.Write.Length);
            il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(int)));

            // if (length <= 0) return;
            Label isNotNullOrEmpty = il.DefineLabel();
            il.EmitLoadLocal(Locals.Write.Length);
            il.EmitLoadInt32(0);
            il.Emit(OpCodes.Bgt_S, isNotNullOrEmpty);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(isNotNullOrEmpty);

            // writer.Write(obj.GetLength([0..Rank]));
            for (int i = 0; i < Rank; i++)
            {
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitLoadArgument(Arguments.Write.T);
                il.EmitLoadInt32(i);
                il.EmitCallvirt(typeof(Array).GetMethod(nameof(Array.GetLength)));
                il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(int)));
            }

            /* Just like in SZArrayProcessor<T>, we can copy the whole memory buffer
             * directly if T is unmanaged, otherwise we go over all the array items
             * and serialize each of them with the right TypeProcessor<T> instance. */
            if (T.IsUnmanaged())
            {
                // writer.Write(MemoryMarshal.CreateSpan(ref obj[0, ..., 0], length));
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitLoadArgument(Arguments.Write.T);
                for (int i = 0; i < Rank; i++)
                    il.EmitLoadInt32(0);
                il.EmitCall(typeof(TArray).GetMethod("Address", BindingFlags.NonPublic | BindingFlags.Instance));
                il.EmitLoadLocal(Locals.Write.Length); 
                il.EmitCall(KnownMembers.Span.RefConstructor(T));
                il.EmitCall(KnownMembers.BinaryWriter.WriteSpanT(T));
            }
            else
            {
                // ref T r0 = ref obj[0, ..., 0];
                il.EmitLoadArgument(Arguments.Write.T);
                for (int i = 0; i < Rank; i++)
                    il.EmitLoadInt32(0);
                il.EmitCall(typeof(TArray).GetMethod("Address", BindingFlags.NonPublic | BindingFlags.Instance));
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
                il.EmitAddOffset(T);
                il.EmitLoadFromAddress(T);
                il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
                il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(T));

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
            il.Emit(OpCodes.Ret);
        }

        /// <inheritdoc/>
        protected override void EmitDeserializer(ILGenerator il)
        {
        }
    }
}

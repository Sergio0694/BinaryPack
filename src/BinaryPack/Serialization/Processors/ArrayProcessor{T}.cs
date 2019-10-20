using System.Reflection.Emit;
using BinaryPack.Extensions.System.Reflection.Emit;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Extensions;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for array types
    /// </summary>
    /// <typeparam name="T">The type of items in arrays to serialize and deserialize</typeparam>
    internal sealed partial class ArrayProcessor<T> : TypeProcessor<T[]?> where T : class, new()
    {
        /// <summary>
        /// Gets the singleton <see cref="ArrayProcessor{T}"/> instance to use
        /// </summary>
        public static ArrayProcessor<T> Instance { get; } = new ArrayProcessor<T>();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            il.DeclareLocalsFromType<Locals.Write>();

            // int length = obj?.Length ?? -1;
            Label
                notNull = il.DefineLabel(),
                lengthLoaded = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Brtrue_S, notNull);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Br_S, lengthLoaded);
            il.MarkLabel(notNull);
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.MarkLabel(lengthLoaded);
            il.EmitStoreLocal(Locals.Write.Length);

            // byte* p = stackalloc byte[sizeof(int)]; *(int*)p = length;
            il.EmitStackalloc(typeof(int));
            il.EmitStoreLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadLocal(Locals.Write.Length);
            il.EmitStoreToAddress(typeof(int));

            // stream.Write(new ReadOnlySpan<byte>(p, 4));
            il.EmitLoadArgument(Arguments.Write.Stream);
            il.EmitLoadLocal(Locals.Write.BytePtr);
            il.EmitLoadInt32(sizeof(int));
            il.Emit(OpCodes.Newobj, KnownMembers.ReadOnlySpan<byte>.UnsafeConstructor);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Write, null);

            // for (int i = 0; i < length; i++) { }
            Label check = il.DefineLabel();
            il.EmitLoadInt32(0);
            il.EmitStoreLocal(Locals.Write.I);
            il.Emit(OpCodes.Br_S, check);
            Label loop = il.DefineLabel();
            il.MarkLabel(loop);

            // SerializationProcessor<T>.Serializer(obj[i], stream);
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitLoadLocal(Locals.Write.I);
            il.Emit(OpCodes.Ldelem_Ref);
            il.EmitLoadArgument(Arguments.Write.Stream);
            il.EmitCall(OpCodes.Call, TypeProcessor<T>._Serializer.MethodInfo, null);

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
            il.Emit(OpCodes.Ret);
        }

        /// <inheritdoc/>
        protected override void EmitDeserializer(ILGenerator il)
        {
            // T[] array; ...;
            il.DeclareLocal(typeof(T).MakeArrayType());
            il.DeclareLocalsFromType<Locals.Read>();

            // Span<byte> span = stackalloc byte[sizeof(int)];
            il.EmitStackalloc(typeof(int));
            il.EmitLoadInt32(sizeof(int));
            il.Emit(OpCodes.Newobj, KnownMembers.Span<byte>.UnsafeConstructor);
            il.EmitStoreLocal(Locals.Read.SpanByte);

            // _ = stream.Read(span);
            il.EmitLoadArgument(Arguments.Read.Stream);
            il.EmitLoadLocal(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Read, null);
            il.Emit(OpCodes.Pop);

            // int length = span.GetPinnableReference();
            il.EmitLoadLocalAddress(Locals.Read.SpanByte);
            il.EmitCall(OpCodes.Call, KnownMembers.Span<byte>.GetPinnableReference, null);
            il.EmitLoadFromAddress(typeof(int));
            il.EmitStoreLocal(Locals.Read.Length);

            // if (length == -1) return array = null;
            Label
                isNotNull = il.DefineLabel(),
                end = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.Length);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brfalse_S, isNotNull);
            il.Emit(OpCodes.Ldnull);
            il.EmitStoreLocal(Locals.Read.Array);
            il.Emit(OpCodes.Br_S, end);

            // else array = new T[length];
            il.MarkLabel(isNotNull);
            il.EmitLoadLocal(Locals.Read.Length);
            il.Emit(OpCodes.Newarr, typeof(T));
            il.EmitStoreLocal(Locals.Read.Array);

            // for (int i = 0; i < length; i++) { }
            Label check = il.DefineLabel();
            il.EmitLoadInt32(0);
            il.EmitStoreLocal(Locals.Read.I);
            il.Emit(OpCodes.Br_S, check);
            Label loop = il.DefineLabel();
            il.MarkLabel(loop);

            // array[i] = SerializationProcessor<T>.Deserializer(stream);
            il.EmitLoadLocal(Locals.Read.Array);
            il.EmitLoadLocal(Locals.Read.I);
            il.EmitLoadArgument(Arguments.Read.Stream);
            il.EmitCall(OpCodes.Call, TypeProcessor<T>._Deserializer.MethodInfo, null);
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

            // return obj;
            il.MarkLabel(end);
            il.EmitLoadLocal(Locals.Read.Array);
            il.Emit(OpCodes.Ret);
        }
    }
}

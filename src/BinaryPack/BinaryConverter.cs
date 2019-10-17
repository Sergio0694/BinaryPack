using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Extensions;

namespace BinaryPack
{
    public delegate void BinarySerializer<in T>(T obj, Stream stream) where T : new();

    public delegate T BinaryDeserializer<out T>(Stream stream) where T : new();

    public static class BinaryConverter
    {
        public static void Serialize<T>(T obj, Stream stream) where T : new()
        {
            DynamicMethod<BinarySerializer<T>>.New(il =>
            {
                IEnumerable<PropertyInfo> properties =
                    from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    where prop.CanRead && prop.CanWrite
                    select prop;

                il.DeclareLocal(typeof(byte*));

                foreach (PropertyInfo property in properties)
                {
                    il.EmitStackalloc(property.PropertyType);
                    il.EmitStoreLocal(0);
                    il.EmitLoadLocal(0);
                    il.Emit(OpCodes.Ldarg_0);
                    il.EmitReadMember(property);
                    il.EmitStoreToAddress(property.PropertyType);
                    il.Emit(OpCodes.Ldarg_1);
                    il.EmitLoadLocal(0);
                    il.EmitLoadInt32(property.PropertyType.GetSize());
                    il.Emit(OpCodes.Newobj, KnownMethods.ReadOnlySpan<byte>.UnsafeConstructor);
                    il.EmitCall(OpCodes.Callvirt, KnownMethods.Stream.Write, null);
                }

                il.Emit(OpCodes.Ret);
            })(obj, stream);
        }

        public static T Deserialize<T>(Stream stream) where T : new()
        {
            return DynamicMethod<BinaryDeserializer<T>>.New(il =>
            {
                IEnumerable<PropertyInfo> properties =
                    from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    where prop.CanRead && prop.CanWrite
                    select prop;

                il.DeclareLocal(typeof(T));
                il.DeclareLocal(typeof(Span<byte>));

                il.Emit(OpCodes.Newobj, KnownMethods.Type<T>.DefaultConstructor);
                il.EmitStoreLocal(0);

                foreach (PropertyInfo property in properties)
                {
                    il.EmitStackalloc(property.PropertyType);
                    il.EmitLoadInt32(property.PropertyType.GetSize());
                    il.Emit(OpCodes.Newobj, KnownMethods.Span<byte>.UnsafeConstructor);
                    il.EmitStoreLocal(1);
                    il.Emit(OpCodes.Ldarg_0);
                    il.EmitLoadLocal(1);
                    il.EmitCall(OpCodes.Callvirt, KnownMethods.Stream.Read, null);
                    il.Emit(OpCodes.Pop);
                    il.EmitLoadLocal(0);
                    il.Emit(OpCodes.Ldloca_S, 1);
                    il.EmitCall(OpCodes.Call, KnownMethods.Span<byte>.GetPinnableReference, null);
                    il.EmitCall(OpCodes.Call, KnownMethods.Unsafe<byte>.As(property.PropertyType), null);
                    il.EmitLoadFromAddress(property.PropertyType);
                    il.EmitWriteMember(property);
                }

                il.EmitLoadLocal(0);
                il.Emit(OpCodes.Ret);
            })(stream);
        }
    }
}

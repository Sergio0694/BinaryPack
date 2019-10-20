using System;
using System.Linq;
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
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for generic models
    /// </summary>
    /// <typeparam name="T">The type of items to handle during serialization and deserialization</typeparam>
    internal sealed partial class ObjectProcessor<T> : TypeProcessor<T> where T : new()
    {
        /// <summary>
        /// Gets the singleton <see cref="ObjectProcessor{T}"/> instance to use
        /// </summary>
        public static ObjectProcessor<T> Instance { get; } = new ObjectProcessor<T>();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            il.DeclareLocals<Locals.Write>();

            /* Perform a null check only if the type is a reference type.
             * In this case, a single byte will be written to the target stream,
             * with a value of -1 if the input item is null, and 1 otherwise. */
            if (!typeof(T).IsValueType)
            {
                // byte* p = stackalloc byte[1];
                il.EmitStackalloc(typeof(byte));
                il.EmitStoreLocal(Locals.Write.BytePtr);
                il.EmitLoadLocal(Locals.Write.BytePtr);

                // *p = obj == null ? -1 : 1;
                Label
                    notNull = il.DefineLabel(),
                    flag = il.DefineLabel();
                il.EmitLoadArgument(Arguments.Write.T);
                il.Emit(OpCodes.Brtrue_S, notNull);
                il.EmitLoadInt32(-1);
                il.Emit(OpCodes.Br_S, flag);
                il.MarkLabel(notNull);
                il.EmitLoadInt32(1);
                il.MarkLabel(flag);
                il.EmitStoreToAddress(typeof(byte));

                // stream.Write(new ReadOnlySpan<byte>(p, 1));
                il.EmitLoadArgument(Arguments.Write.Stream);
                il.EmitLoadLocal(Locals.Write.BytePtr);
                il.EmitLoadInt32(sizeof(byte));
                il.Emit(OpCodes.Newobj, KnownMembers.ReadOnlySpan<byte>.UnsafeConstructor);
                il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Write, null);

                // if (obj == null) return;
                Label skip = il.DefineLabel();
                il.EmitLoadArgument(Arguments.Write.T);
                il.Emit(OpCodes.Brtrue_S, skip);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(skip);
            }

            // Properties serialization
            foreach (PropertyInfo property in
                from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where prop.CanRead && prop.CanWrite
                select prop)
            {
                /* First special case, for unmanaged value types.
                 * Here we can just copy the property value directly to a
                 * local buffer of the right size, then cast it to a ReadOnlySpan<byte>
                 * span and write it to the target stream. No particular care is required. */
                if (property.PropertyType.IsUnmanaged())
                {
                    // byte* p = stackalloc byte[Unsafe.SizeOf<TProperty>()];
                    il.EmitStackalloc(property.PropertyType);
                    il.EmitStoreLocal(Locals.Write.BytePtr);

                    // Unsafe.Write<TProperty>(p, obj.Property);
                    il.EmitLoadLocal(Locals.Write.BytePtr);
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.EmitStoreToAddress(property.PropertyType);

                    // stream.Write(new ReadOnlySpan<byte>(p, Unsafe.SizeOf<TProperty>()));
                    il.EmitLoadArgument(Arguments.Write.Stream);
                    il.EmitLoadLocal(Locals.Write.BytePtr);
                    il.EmitLoadInt32(property.PropertyType.GetSize());
                    il.Emit(OpCodes.Newobj, KnownMembers.ReadOnlySpan<byte>.UnsafeConstructor);
                    il.EmitCall(OpCodes.Callvirt, KnownMembers.Stream.Write, null);
                }
                else if (property.PropertyType == typeof(string))
                {
                    /* Second special case, for string values. Here we just need to
                     * load the string property and then invoke the string processor, which
                     * will handle all the possible cases like null values, empty strings, etc. */
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.EmitLoadArgument(Arguments.Write.Stream);
                    il.EmitCall(OpCodes.Call, StringProcessor.Instance.SerializerInfo.MethodInfo, null);
                }
                else if (property.PropertyType.IsArray)
                {
                    /* Third special case, for array types. Like with strings, we only need
                     * to load the property valaue and then delegate the rest of the
                     * serialization to the appropriate ArrayProcessor<T> instance, which
                     * is retrieved through reflection from the type of elements in the current property. */
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.EmitLoadArgument(Arguments.Write.Stream);
                    il.EmitCall(OpCodes.Call, KnownMembers.ArrayProcessor.SerializerInfo(property.PropertyType.GetElementType()), null);
                }
                else
                {
                    // Just use another ObjectProcessor<T> instance for all other property types
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.EmitLoadArgument(Arguments.Write.Stream);
                    il.EmitCall(OpCodes.Call, KnownMembers.ObjectProcessor.SerializerInfo(property.PropertyType), null);
                }
            }

            il.Emit(OpCodes.Ret);
        }

        /// <inheritdoc/>
        protected override void EmitDeserializer(ILGenerator il)
        {
            // T obj; ...;
            il.DeclareLocal(typeof(T));
            il.DeclareLocals<Locals.Read>();

            // Initialize T obj to either new T() or null
            il.EmitDeserializeEmptyInstanceOrNull<T>();

            // Skip the deserialization if the instance in null
            Label end = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.T);
            il.Emit(OpCodes.Brfalse_S, end);

            // Deserialize all the contained properties
            foreach (PropertyInfo property in
                from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where prop.CanRead && prop.CanWrite
                select prop)
            {
                if (property.PropertyType.IsUnmanaged()) il.EmitDeserializeUnmanagedProperty(property);
                else if (property.PropertyType == typeof(string)) il.EmitDeserializeStringProperty(property);
                else if (property.PropertyType.IsArray && property.PropertyType.GetElementType().IsUnmanaged()) il.EmitDeserializeUnmanagedArrayProperty(property);
                else if (property.PropertyType.IsArray && !property.PropertyType.GetElementType().IsValueType) { } // TODO
                else if (!property.PropertyType.IsValueType)
                {
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.Stream);
                    il.EmitCall(OpCodes.Call, KnownMembers.SerializationProcessor.DeserializerInfo(property.PropertyType), null);
                    il.EmitWriteMember(property);
                }
                else throw new InvalidOperationException($"Property of type {property.PropertyType} not supported");
            }

            // return obj;
            il.MarkLabel(end);
            il.EmitLoadLocal(Locals.Read.T);
            il.Emit(OpCodes.Ret);
        }
    }
}

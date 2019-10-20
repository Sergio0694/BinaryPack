using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Serialization.Attributes;

namespace BinaryPack.Serialization.Extensions
{
    /// <summary>
    /// A <see langword="class"/> that provides serialization extension methods for the <see langword="ILGenerator"/> type
    /// </summary>
    internal static partial class ILGeneratorExtensions
    {
        /// <summary>
        /// Declares local variables with the types specified in the public members of a given type
        /// </summary>
        /// <typeparam name="T">The type to use to retrieve the types of locals to declare</typeparam>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        public static void DeclareLocals<T>(this ILGenerator il) where T : Enum
        {
            foreach (Type type in
                from field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static)
                where field.IsLiteral && !field.IsInitOnly
                let attribute = field.GetCustomAttributes().OfType<LocalTypeAttribute>().FirstOrDefault()
                where attribute != null
                select attribute.Type)
            {
                il.DeclareLocal(type);
            }
        }

        /// <summary>
        /// Declares local variables with the types specified in the public members of a given type
        /// </summary>
        /// <typeparam name="T">The type to use to retrieve the types of locals to declare</typeparam>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        public static void DeclareLocalsFromType<T>(this ILGenerator il)
        {
            foreach (Type type in
                from field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static)
                where field.IsLiteral && !field.IsInitOnly
                let attribute = field.GetCustomAttributes().OfType<LocalTypeAttribute>().FirstOrDefault()
                where attribute != null
                select attribute.Type)
            {
                il.DeclareLocal(type);
            }
        }
    }
}

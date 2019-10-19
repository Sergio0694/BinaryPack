using System;
using System.Linq;
using System.Reflection.Emit;
using BinaryPack.Extensions;
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
        public static void DeclareLocalsFromType<T>(this ILGenerator il)
        {
            LocalTypeAttribute[] attributes = typeof(T).GetAttributes<LocalTypeAttribute>().ToArray();
            if (attributes.Length == 0) throw new InvalidOperationException($"Type [{typeof(T)}] doesn't contain valid members");

            // Automatically declare the locals from the extracted attributes
            foreach (LocalTypeAttribute attribute in attributes)
            {
                il.DeclareLocal(attribute.Type);
            }
        }
    }
}

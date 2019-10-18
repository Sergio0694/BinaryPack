using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {

        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="Unsafe"/> type
        /// </summary>
        public static class Unsafe
        {
            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="System.Runtime.CompilerServices.Unsafe.SizeOf{T}"/> method
            /// </summary>
            public static MethodInfo SizeOf { get; } = (
                from method in typeof(System.Runtime.CompilerServices.Unsafe).GetMethods(BindingFlags.Public | BindingFlags.Static)
                where method.Name.Equals(nameof(System.Runtime.CompilerServices.Unsafe.SizeOf))
                select method).First();
        }
    }
}

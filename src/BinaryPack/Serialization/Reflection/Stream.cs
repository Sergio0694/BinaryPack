using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMethods
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="Stream"/> type
        /// </summary>
        public static class Stream
        {
            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="System.IO.Stream.Read(System.Span{byte})"/> method
            /// </summary>
            public static MethodInfo Read { get; } = (
                from method in typeof(System.IO.Stream).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                where method.Name.Equals(nameof(System.IO.Stream.Read))
                let args = method.GetParameters()
                where args.Length == 1
                select method).First();

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> instance mapping the <see cref="System.IO.Stream.Write(System.ReadOnlySpan{byte})"/> method
            /// </summary>
            public static MethodInfo Write { get; } = (
                from method in typeof(System.IO.Stream).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                where method.Name.Equals(nameof(System.IO.Stream.Write))
                let args = method.GetParameters()
                where args.Length == 1
                select method).First();
        }
    }
}

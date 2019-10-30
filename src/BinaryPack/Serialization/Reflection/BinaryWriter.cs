using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="Buffers.BinaryWriter"/> type
        /// </summary>
        public static class BinaryWriter
        {
            /// <summary>
            /// The <see cref="MethodInfo"/> instance mapping the <see cref="Buffers.BinaryWriter.Write{T}(T)"/> method
            /// </summary>
            private static readonly MethodInfo _WriteT = (
                from method in typeof(Buffers.BinaryWriter).GetMethods()
                where method.Name.Equals(nameof(Buffers.BinaryWriter.Write))
                let parameters = method.GetParameters()
                let generics = method.GetGenericArguments()
                where parameters.Length == 1 &&
                      generics.Length == 1 &&
                      parameters[0].ParameterType == generics[0]
                select method).First();

            /// <summary>
            /// Gets a generic <see cref="MethodInfo"/> instance mapping the <see cref="Buffers.BinaryWriter.Write{T}(T)"/> method
            /// </summary>
            [Pure]
            public static MethodInfo WriteT(Type type) => _WriteT.MakeGenericMethod(type);

            /// <summary>
            /// The <see cref="MethodInfo"/> instance mapping the <see cref="Buffers.BinaryWriter.Write{T}(Span{T})"/> method
            /// </summary>
            private static readonly MethodInfo _WriteSpanT = (
                from method in typeof(Buffers.BinaryWriter).GetMethods()
                where method.Name.Equals(nameof(Buffers.BinaryWriter.Write))
                let parameters = method.GetParameters()
                let generics = method.GetGenericArguments()
                where parameters.Length == 1 &&
                      generics.Length == 1 &&
                      parameters[0].ParameterType == typeof(Span<>).MakeGenericType(generics[0])
                select method).First();

            /// <summary>
            /// Gets a generic <see cref="MethodInfo"/> instance mapping the <see cref="Buffers.BinaryWriter.Write{T}(Span{T})"/> method
            /// </summary>
            [Pure]
            public static MethodInfo WriteSpanT(Type type) => _WriteSpanT.MakeGenericMethod(type);
        }
    }
}

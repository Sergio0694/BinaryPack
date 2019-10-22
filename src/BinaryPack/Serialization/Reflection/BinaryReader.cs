using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace BinaryPack.Serialization.Reflection
{
    internal static partial class KnownMembers
    {
        /// <summary>
        /// A <see langword="class"/> containing methods from the <see cref="Buffers.BinaryReader"/> type
        /// </summary>
        public static class BinaryReader
        {
            /// <summary>
            /// The <see cref="MethodInfo"/> instance mapping the <see cref="Buffers.BinaryReader.Read{T}()"/> method
            /// </summary>
            private static readonly MethodInfo _WriteT = (
                from method in typeof(Buffers.BinaryReader).GetMethods()
                where method.Name.Equals(nameof(Buffers.BinaryReader.Read))
                let parameters = method.GetParameters()
                let generics = method.GetGenericArguments()
                where parameters.Length == 0 &&
                      generics.Length == 1
                select method).First();

            /// <summary>
            /// Gets a generic <see cref="MethodInfo"/> instance mapping the <see cref="Buffers.BinaryReader.Read{T}()"/> method
            /// </summary>
            [Pure]
            public static MethodInfo ReadT(Type type) => _WriteT.MakeGenericMethod(type);

            /// <summary>
            /// The <see cref="MethodInfo"/> instance mapping the <see cref="Buffers.BinaryReader.Read{T}(Span{T})"/> method
            /// </summary>
            private static readonly MethodInfo _WriteSpanT = (
                from method in typeof(Buffers.BinaryReader).GetMethods()
                where method.Name.Equals(nameof(Buffers.BinaryReader.Read))
                let parameters = method.GetParameters()
                let generics = method.GetGenericArguments()
                where parameters.Length == 1 &&
                      generics.Length == 1 &&
                      parameters[0].ParameterType == typeof(Span<>).MakeGenericType(generics[0])
                select method).First();

            /// <summary>
            /// Gets a generic <see cref="MethodInfo"/> instance mapping the <see cref="Buffers.BinaryReader.Read{T}(Span{T})"/> method
            /// </summary>
            [Pure]
            public static MethodInfo ReadSpanT(Type type) => _WriteSpanT.MakeGenericMethod(type);
        }
    }
}

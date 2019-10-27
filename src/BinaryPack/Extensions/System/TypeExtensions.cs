using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BinaryPack.Attributes;
using BinaryPack.Enums;

namespace System
{
    /// <summary>
    /// A <see langword="class"/> that provides extension methods for the <see cref="Type"/> type
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Enumerates all the members of the input <see cref="Type"/> that are supported for serialization
        /// </summary>
        /// <param name="type">The input type to analyze</param>
        [Pure]
        public static IReadOnlyCollection<MemberInfo> GetSerializableMembers(this Type type)
        {
            BinarySerializationAttribute attribute = type.GetCustomAttribute<BinarySerializationAttribute>();
            SerializationMode mode = attribute?.Mode ?? SerializationMode.Properties;
            IReadOnlyCollection<MemberInfo> members = (mode switch
            {
                /* If the mode is set to explicit, we query all the members of the
                 * target type, both public and not public, and return the ones that
                 * would respect either the Properties or Fields modes, and that
                 * are explicitly marked as serializable members. */
                SerializationMode.Explicit =>

                from member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                where (member.MemberType == MemberTypes.Property ||
                       member.MemberType == MemberTypes.Field) &&
                      member.IsDefined(typeof(SerializableMemberAttribute)) &&
                      (member is FieldInfo fieldInfo && !fieldInfo.IsInitOnly && !fieldInfo.IsLiteral ||
                       member is PropertyInfo propertyInfo && propertyInfo.CanRead && propertyInfo.CanWrite)
                orderby member.Name
                select member,

                /* For all other modes, we first retrieve all the candidate members from the
                 * input type - that is, public instance members by default, plus non public members
                 * too if the PublicMembersOnly flag is not selected. Then we filter out either
                 * properties or fields (or neither of them) as requested, skip the members
                 * that are marked as non serializable and finally select the available members as before. */
                _ =>

                from member in type.GetMembers(
                    BindingFlags.Public | BindingFlags.Instance |
                    (mode.HasFlag(SerializationMode.NonPublicMembers) ? BindingFlags.NonPublic : BindingFlags.Default))
                where (mode.HasFlag(SerializationMode.Properties) && member.MemberType == MemberTypes.Property ||
                       mode.HasFlag(SerializationMode.Fields) && member.MemberType == MemberTypes.Field) &&
                      !member.IsDefined(typeof(IgnoredMemberAttribute)) &&
                      (member is FieldInfo fieldInfo && !fieldInfo.IsInitOnly && !fieldInfo.IsLiteral ||
                       member is PropertyInfo propertyInfo && propertyInfo.CanRead && propertyInfo.CanWrite)
                orderby member.Name
                select member
            }).ToArray();

            return members;
        }

        /// <summary>
        /// Gets the syze in bytes of the given type
        /// </summary>
        /// <param name="type">The input type to analyze</param>
        [Pure]
        public static int GetSize(this Type type) => (int)typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf)).MakeGenericMethod(type).Invoke(null, null);

        /// <summary>
        /// Helper <see langword="class"/> for the <see cref="IsUnmanaged"/> method
        /// </summary>
        /// <typeparam name="T">The type to check against the <see langword="unmanaged"/> constraint</typeparam>
        private static class _IsUnmanaged<T> where T : unmanaged { }

        /// <summary>
        /// Checks whether or not the input type respects the <see langword="unmanaged"/> constraint
        /// </summary>
        /// <param name="type">The input type to analyze</param>
        [Pure]
        public static bool IsUnmanaged(this Type type)
        {
            try
            {
                _ = typeof(_IsUnmanaged<>).MakeGenericType(type);
                return true;
            }
            catch (ArgumentException)
            {
                // Not unmanaged
                return false;
            }
        }

        /// <summary>
        /// Checks whether or not the input type is a 1D array type
        /// </summary>
        /// <param name="type">The input type to analyze</param>
        [Pure]
        public static bool IsLinearArrayType(this Type type) => type.IsArray &&
                                                                type == type.GetElementType().MakeArrayType();

        /// <summary>
        /// Checks whether or not the input type is a generic type that matches a given definition
        /// </summary>
        /// <param name="type">The input type to analyze</param>
        /// <param name="target">The generic type to compare the input type to</param>
        [Pure]
        public static bool IsGenericType(this Type type, Type target) => type.IsGenericType &&
                                                                         type.GetGenericTypeDefinition() == target;

        /// <summary>
        /// Gets a generic instantiation of a nested type for a given generic type
        /// </summary>
        /// <param name="type">The input generic type</param>
        /// <param name="name">The name of the nested <see cref="Type"/> to retrieve</param>
        [Pure]
        public static Type GetGenericNestedType(this Type type, string name)
        {
            Type nestedType = type.GetNestedType(name, BindingFlags.Public | BindingFlags.NonPublic);
            return nestedType.MakeGenericType(type.GenericTypeArguments);
        }
    }
}

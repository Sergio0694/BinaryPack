using System.Diagnostics.Contracts;

namespace System.Reflection
{
    /// <summary>
    /// A <see langword="class"/> that provides extension methods for the <see cref="MemberInfo"/> type
    /// </summary>
    public static class MemberInfoExtensions
    {
       /// <summary>
       /// Gets the type of the value of the given <see cref="MemberInfo"/> instance
       /// </summary>
       /// <param name="memberInfo">The input <see cref="MemberInfo"/> to analyze</param>
        [Pure]
        public static Type GetMemberType(this MemberInfo memberInfo) => memberInfo switch
        {
            PropertyInfo propertyInfo => propertyInfo.PropertyType,
            FieldInfo fieldInfo => fieldInfo.FieldType,
            _ => throw new ArgumentException($"Invalid member of type {memberInfo.GetType()}")
        };
    }
}

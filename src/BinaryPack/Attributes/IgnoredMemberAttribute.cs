using System;

namespace BinaryPack.Attributes
{
    /// <summary>
    /// A custom <see cref="Attribute"/> that marks an member that doesn't support serialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class IgnoredMemberAttribute : Attribute { }
}

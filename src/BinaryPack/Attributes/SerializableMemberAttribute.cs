using System;

namespace BinaryPack.Attributes
{
    /// <summary>
    /// A custom <see cref="Attribute"/> that marks an member that supports serialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class SerializableMemberAttribute : Attribute { }
}

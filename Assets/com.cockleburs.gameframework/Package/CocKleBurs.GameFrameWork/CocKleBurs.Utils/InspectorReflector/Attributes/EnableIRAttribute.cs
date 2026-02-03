using System;

namespace CockleBurs.GameFramework.Utility
{
    /// <summary>
    ///     Classes marked with this attribute can be inspected by the IR.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EnableIRAttribute : Attribute
    {
    }
}
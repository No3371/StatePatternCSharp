using System;

namespace BAStudio.StatePattern
{
    /// <summary>
    /// The user property must have a setter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AutoComponentAttribute : Attribute {}
}
using System;

namespace BAStudio.StatePattern
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class AutoComponentAttribute : Attribute {}
}
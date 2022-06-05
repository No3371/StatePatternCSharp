using System;

namespace BAStudio.StatePattern
{
    /// <summary>
    /// This attribute tells StatMachine skip the Reflection works to automatically fills in members marked AutoComponents.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class DisableAutoComponents : Attribute {}
}
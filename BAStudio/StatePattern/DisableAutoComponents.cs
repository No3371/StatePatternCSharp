using System;

namespace BAStudio.StatePattern
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class DisableAutoComponents : Attribute {}
}
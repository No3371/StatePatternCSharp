using System;

namespace BAStudio.StatePattern
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class DontAutoAssignComponent : Attribute {}
}
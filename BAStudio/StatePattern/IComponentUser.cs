using System;

namespace BAStudio.StatePattern
{
    public interface IComponentUser
	{
		void OnComponentSupplied (Type t, object o);
	}
}
using System;

namespace BAStudio.StatePattern
{
    public interface IComponentUser
	{
		/// <summary>
		/// <para>This interface is <b>required</b> for a StateMachine to deliver components to your state.</para>
		/// <para>If the IComponentUser has [DisableAutoComponent] attribute, this get called for every component stored in the statemachine.</para>
		/// <para>Otherwise, this get called for every component automatically supplied to the IComponentUser's [AutoComponent]s.</para>
		/// </summary>
		void OnComponentSupplied (Type t, object o);
	}
}
using System;

namespace BAStudio.StatePattern
{
    public interface IParameterizedState<T> 
	{
		void OnEntered(StateMachine<T> machine, State<T> previous, IStateParameter<T> parameter);
	}

	public interface IComponentUser
	{
		void OnComponentSupplied (Type t, object o);
	}
}
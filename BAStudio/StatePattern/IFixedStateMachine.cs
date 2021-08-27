using System.Collections.Generic;

namespace BAStudio.StatePattern
{
    public interface IFixedStateMachine<ID, T> : IStateMachine<T> where ID : System.Enum
	{
		ID CurrentStateID { get; }
		event System.Action<ID, ID> OnStateIDChanging, OnStateIDChanged;
		IReadOnlyDictionary<ID, IState<T>> StateIndex { get; }
		void ChangeState (ID state);
		void ChangeState<P> (ID state, P parameter) where P : IStateParameter<T>;
	}

}
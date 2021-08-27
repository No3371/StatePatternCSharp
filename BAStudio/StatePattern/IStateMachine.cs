namespace BAStudio.StatePattern
{
    public interface IStateMachine<T>
	{
		T Target { get; }
		bool AllowUpdate { get; set; }
		bool WillPassEvent { get; }
		bool ChangingState { get; }
		IState<T> CurrentState { get; }
		event System.Action<IState<T>, IState<T>> OnStateChanging, OnStateChanged;
		bool InvokeEvent (IStateEvent<T> stateEvent);
		void Update ();
		void ChangeState (IState<T> state);
		void ChangeState<S>() where S : IState<T>, new();
		void ChangeState<P> (IState<T> state, P parameter) where P : IStateParameter<T>;
		void ChangeState<S, P>(P parameter) where S : IState<T>, new() where P : IStateParameter<T>;
	}

}
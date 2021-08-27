namespace BAStudio.StatePattern
{
    public interface IState<T>
	{
	    bool AllowUpdate { get; }
		void OnEntered (IStateMachine<T> machine);
		void Update (IStateMachine<T> machine);
		void OnLeaving (IStateMachine<T> machine);
		void ReceiveEvent (IStateMachine<T> machine, IStateEvent<T> stateEvent);
	}
}
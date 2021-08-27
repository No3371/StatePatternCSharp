namespace BAStudio.StatePattern
{
    public interface IDynamicStateMachine<T> : IStateMachine<T>
	{
		IState<T> CurrentState { get; }
		event System.Action<IState<T>, IState<T>> OnStateChanging, OnStateChanged;
		void ChangeState (IState<T> state);
		void ChangeState (IState<T> state, IStateParameter<T> parameter);
	}

}
namespace BAStudio.StatePattern
{
    public interface IParameterizedState<T>
	{
		void OnEntered(StateMachine<T> machine, StateMachine<T>.State previous, IStateParameter<T> parameter);
	}
}
namespace BAStudio.StatePattern
{
    public partial class StateMachine<T>
    {
        public abstract class State
		{
			public abstract void OnEntered(StateMachine<T> machine, State previous, T context, object parameter = null);
			public abstract void Update(StateMachine<T> machine, T context);
			public abstract void OnLeaving(StateMachine<T> machine, State next, T context, object parameter = null);
        }
    }
}
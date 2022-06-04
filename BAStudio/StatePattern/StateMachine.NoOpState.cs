namespace BAStudio.StatePattern
{
    public partial class StateMachine<T>
    {
        public sealed class NoOpState : State
		{
			public override void OnEntered(StateMachine<T> machine, State previous, T context, object parameter = null) {}
			public override void Update(StateMachine<T> machine, T context) {}
			public override void OnLeaving(StateMachine<T> machine, State next, T context, object parameter = null) {}
		}
    }
}
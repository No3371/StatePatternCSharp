namespace BAStudio.StatePattern
{
    public partial class StateMachine<T>
    {
        public sealed class NoOpState : State
		{
			public override void OnEntered(StateMachine<T> machine, State previous, T subject, object parameter = null) {}
			public override void Update(StateMachine<T> machine, T subject) {}
			public override void OnLeaving(StateMachine<T> machine, State next, T subject, object parameter = null) {}
		}
    }
}
namespace BAStudio.StatePattern
{
    public partial class StateMachine<T>
    {
        public abstract class State
		{
			public bool AllowUpdate { get; protected set; } = true;
			public abstract void OnEntered(StateMachine<T> machine, State previous, T context);
			public abstract void Update(StateMachine<T> machine, T context);
			public abstract void OnLeaving(StateMachine<T> machine, State next, T context);
			public virtual void ReceiveEvent(StateMachine<T> machine, IStateEvent<T> stateEvent, T context) {}
		}

		public sealed class NoOpState : State
		{
			public override void OnEntered(StateMachine<T> machine, State previous, T context) {}
			public override void Update(StateMachine<T> machine, T context) {}
			public override void OnLeaving(StateMachine<T> machine, State next, T context) {}
			public override void ReceiveEvent(StateMachine<T> machine, IStateEvent<T> stateEvent, T context) {}
		}
	}
}
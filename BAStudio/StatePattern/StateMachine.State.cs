namespace BAStudio.StatePattern
{
    public partial class StateMachine<T>
    {
        public abstract class State
		{
			public bool AllowUpdate { get; protected set; } = true;
			/// <summary>
			/// It is abstract because all states should be very clear about what they do.
			/// </summary>
			public abstract void OnEntered(StateMachine<T> machine, State previous, T context, IStateParameter<T> parameter = null);
			/// <summary>
			/// It is abstract because all states should be very clear about what they do.
			/// </summary>
			public abstract void Update(StateMachine<T> machine, T context);
			/// <summary>
			/// It is abstract because all states should be very clear about what they do.
			/// </summary>
			public abstract void OnLeaving(StateMachine<T> machine, State next, T context);
			public virtual void ReceiveEvent(StateMachine<T> machine, IStateEvent<T> stateEvent, T context) {}
		}

		public sealed class NoOpState : State
		{
			public override void OnEntered(StateMachine<T> machine, State previous, T context, IStateParameter<T> parameter = null) {}
			public override void Update(StateMachine<T> machine, T context) {}
			public override void OnLeaving(StateMachine<T> machine, State next, T context) {}
			public override void ReceiveEvent(StateMachine<T> machine, IStateEvent<T> stateEvent, T context) {}
		}

        public class TransitionState<NEXT> : State where NEXT : State, new()
        {
			public TransitionState(System.Func<bool> condition) : base() {
                Condition = condition;
            }

            public System.Func<bool> Condition { get; }

            public override void OnEntered(StateMachine<T> machine, State previous, T context, IStateParameter<T> parameter = null) {}

            public override void OnLeaving(StateMachine<T> machine, State next, T context) {}

            public override void Update(StateMachine<T> machine, T context)
            {
				if (Condition()) machine.ChangeState<NEXT>();
            }
        }
    }
}
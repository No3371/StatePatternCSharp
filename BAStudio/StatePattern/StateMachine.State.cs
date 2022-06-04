namespace BAStudio.StatePattern
{
    public partial class StateMachine<T>
    {
        public abstract class State
		{
			/// <summary>
			/// It is abstract because all states should be very clear about what they do.
			/// </summary>
			public abstract void OnEntered(StateMachine<T> machine, State previous, T context, object parameter = null);
			/// <summary>
			/// It is abstract because all states should be very clear about what they do.
			/// </summary>
			public abstract void Update(StateMachine<T> machine, T context);
			/// <summary>
			/// It is abstract because all states should be very clear about what they do.
			/// </summary>
			public abstract void OnLeaving(StateMachine<T> machine, State next, T context, object parameter = null);
        }
    }
}
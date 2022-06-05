namespace BAStudio.StatePattern
{
    public partial class StateMachine<T>
    {
        public abstract class State
		{
			public abstract void OnEntered(StateMachine<T> machine, State previous, T context, object parameter = null);
			/// <summary>
			/// <para> Only happens in StateMachine<T>.Update().</para>
			/// <para> It's not guaranteed that this will get called between `OnEntered()` and `OnLeaving()`, because these 2 methods can `ChangeState()` too.</para>
			/// </summary>
			public abstract void Update(StateMachine<T> machine, T context);
			public abstract void OnLeaving(StateMachine<T> machine, State next, T context, object parameter = null);
        }
    }
}
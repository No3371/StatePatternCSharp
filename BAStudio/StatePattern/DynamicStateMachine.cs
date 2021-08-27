using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace BAStudio.StatePattern
{

    public abstract class DynamicStateMachine<T> : IStateMachine<T>
    {
		public event System.Action<string> DebugOutput;
        public T Target { get; }
        public IState<T> CurrentState { get; protected set; }
        public bool AllowUpdate { get; set; }
        public bool WillPassEvent { get; protected set; }
        public bool ChangingState { get; protected set; }
        public event Action<IState<T>, IState<T>> OnStateChanging;
        public event Action<IState<T>, IState<T>> OnStateChanged;
        public virtual void ChangeState(IState<T> state)
        {
			var prev = CurrentState;
			PreStateChange(CurrentState, state);
			CurrentState = state;
			CurrentState?.OnEntered();
			PostStateChange(prev);
        }

        public virtual void ChangeState<P>(IState<T> state, P parameter) where P : IStateParameter<T>
        {
			var prev = CurrentState;
			PreStateChange(CurrentState, state);
			CurrentState = state;
			if (CurrentState is IParameterConsumer<T, P> pc) pc?.OnEntered(parameter);
			else CurrentState?.OnEntered();
			PostStateChange(prev);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void PreStateChange (IState<T> fromState, IState<T> toState)
		{
			WillPassEvent = false;
			ChangingState = true;
			DebugOutput?.Invoke("StateMachine<" + Target.GetType().Name + "> is switching to: " + toState.GetType().Name);
			fromState?.OnLeaving();
			OnStateChanging(fromState, toState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void PostStateChange (IState<T> fromState)
		{
			OnStateChanged(fromState, CurrentState);
			WillPassEvent = true;
			ChangingState = false;
		}
		
		public virtual void Update ()
		{
			if (!AllowUpdate) return;
			if (Target == null) throw new System.NullReferenceException("Target is null.");
			if (CurrentState == null) throw new System.NullReferenceException("CurrentState is null. Did you set a state after instantiate this controller?");
			if (CurrentState.AllowUpdate) CurrentState.Update();
		}

        public virtual bool InvokeEvent(IStateEvent<T> stateEvent)
        {
			if (!WillPassEvent || !stateEvent.CanInvoke(CurrentState)) return false;
			CurrentState?.ReceiveEvent(stateEvent);
			return true;
        }
	}

}
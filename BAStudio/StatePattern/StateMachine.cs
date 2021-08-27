using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BAStudio.StatePattern
{

    public abstract class StateMachine<T> : IStateMachine<T>
    {
        protected StateMachine(T target)
        {
            Target = target;
            AllowUpdate = true;
            WillPassEvent = false;
            ChangingState = false;
        }

        public event System.Action<string> DebugOutput;
        public T Target { get; }
        public IState<T> CurrentState { get; protected set; }
        public bool AllowUpdate { get; set; }
        public bool WillPassEvent { get; protected set; }
        public bool ChangingState { get; protected set; }
        public event Action<IState<T>, IState<T>> OnStateChanging;
        public event Action<IState<T>, IState<T>> OnStateChanged;
        private Dictionary<Type, IState<T>> AutoStateCache { get; set; }
        public virtual void ChangeState(IState<T> state)
        {
			var prev = CurrentState;
			PreStateChange(CurrentState, state);
			CurrentState = state;
			CurrentState?.OnEntered(this);
			PostStateChange(prev);
        }

        public virtual void ChangeState<P>(IState<T> state, P parameter) where P : IStateParameter<T>
        {
			var prev = CurrentState;
			PreStateChange(CurrentState, state);
			CurrentState = state;
			if (CurrentState is IParameterConsumer<T, P> pc) pc?.OnEntered(parameter);
			else CurrentState?.OnEntered(this);
			PostStateChange(prev);
        }
        public virtual void ChangeState<S>() where S : IState<T>, new()
		{
			if (AutoStateCache == null) AutoStateCache = new Dictionary<Type, IState<T>>();
			if (!AutoStateCache.ContainsKey(typeof(S))) AutoStateCache.Add(typeof(S), new S());
			ChangeState(AutoStateCache[typeof(S)]);
		} 
        public virtual void ChangeState<S, P>(P parameter) where S : IState<T>, new() where P : IStateParameter<T>
		{
			if (AutoStateCache == null) AutoStateCache = new Dictionary<Type, IState<T>>();
			if (!AutoStateCache.ContainsKey(typeof(S))) AutoStateCache.Add(typeof(S), new S());
			ChangeState(AutoStateCache[typeof(S)], parameter);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void PreStateChange (IState<T> fromState, IState<T> toState)
		{
			WillPassEvent = false;
			ChangingState = true;
			DebugOutput?.Invoke("StateMachine<" + Target.GetType().Name + "> is switching to: " + toState.GetType().Name);
			fromState?.OnLeaving(this);
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
			if (CurrentState.AllowUpdate) CurrentState.Update(this);
		}

        public virtual bool InvokeEvent(IStateEvent<T> stateEvent)
        {
			if (!WillPassEvent || !stateEvent.CanInvoke(CurrentState)) return false;
			CurrentState?.ReceiveEvent(this, stateEvent);
			return true;
        }
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BAStudio.StatePattern
{
    public class StateMachine<T>
    {
        public StateMachine(T target)
        {
            Target = target;
            AllowUpdate = true;
            WillPassEvent = false;
            ChangingState = false;
        }

        public event System.Action<string> DebugOutput;
        public T Target { get; }
        public State<T> CurrentState { get; protected set; }
        public virtual bool AllowUpdate { get; set; }
        public virtual bool WillPassEvent { get; protected set; }
        public virtual bool ChangingState { get; protected set; }
        public event Action<State<T>, State<T>> OnStateChanging;
        public event Action<State<T>, State<T>> OnStateChanged;
        protected Dictionary<Type, State<T>> AutoStateCache { get; set; }

        public Dictionary<Type, object> Components { get; private set; }
        public virtual void ChangeState(State<T> state)
        {
			var prev = CurrentState;
			PreStateChange(CurrentState, state);
			CurrentState = state;
			DeliverComponents(CurrentState);
			CurrentState?.OnEntered(this, prev);
			PostStateChange(prev);
        }

        public virtual void ChangeState<P>(State<T> state, P parameter) where P : IStateParameter<T>
        {
			var prev = CurrentState;
			PreStateChange(CurrentState, state);
			CurrentState = state;
			DeliverComponents(CurrentState);
			if (CurrentState is IParameterizedState<T> pc) pc?.OnEntered(this, prev, parameter);
			else CurrentState?.OnEntered(this, prev);
			PostStateChange(prev);
        }
        public virtual void ChangeState<S>() where S : State<T>, new()
		{
			if (AutoStateCache == null) AutoStateCache = new Dictionary<Type, State<T>>();
			if (!AutoStateCache.ContainsKey(typeof(S))) AutoStateCache.Add(typeof(S), new S());
			ChangeState(AutoStateCache[typeof(S)]);
		} 
        public virtual void ChangeState<S, P>(P parameter) where S : State<T>, new() where P : IStateParameter<T>
		{
			if (AutoStateCache == null) AutoStateCache = new Dictionary<Type, State<T>>();
			if (!AutoStateCache.ContainsKey(typeof(S))) AutoStateCache.Add(typeof(S), new S());
			ChangeState(AutoStateCache[typeof(S)], parameter);
		}
		void DeliverComponents (State<T> state)
		{
			if (state is IComponentUser cu)
			{
				foreach (var kvp in Components)
					cu.OnComponentSupplied(kvp.Key, kvp.Value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void PreStateChange (State<T> fromState, State<T> toState)
		{
			WillPassEvent = false;
			ChangingState = true;
			DebugOutput?.Invoke("StateMachine<" + Target.GetType().Name + "> is switching to: " + toState.GetType().Name);
			fromState?.OnLeaving(this, toState);
			OnStateChanging?.Invoke(fromState, toState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void PostStateChange (State<T> fromState)
		{
			OnStateChanged?.Invoke(fromState, CurrentState);
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
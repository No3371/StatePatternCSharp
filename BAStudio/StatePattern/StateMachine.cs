using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BAStudio.StatePattern
{
    public partial class StateMachine<T>
    {
		static StringBuilder DebugStringBuilder { get; set; }
        public event System.Action<string> DebugOutput;
        public StateMachine(T target)
        {
            Target = target;
            AllowUpdate = true;
            ChangingState = false;
        }

        public T Target { get; }
        public State CurrentState { get; protected set; }
        public virtual bool AllowUpdate { get; set; }
        public virtual bool ChangingState { get; protected set; }
        public event Action<State, State> OnStateChanging;
        public event Action<State, State> OnStateChanged;
        protected Dictionary<Type, State> AutoStateCache { get; set; }
        Dictionary<Type, object> Components { get; set; }
		public void SetComponent<PT, CT> (CT obj) where CT : PT
		{
			if (Components == null) Components = new Dictionary<Type, object>();
			Components[typeof(PT)] = obj;
		}

        public virtual void ChangeState(State state)
        {
			var prev = CurrentState;
			PreStateChange(CurrentState, state);
			CurrentState = state;
			DeliverComponents(CurrentState);
			CurrentState?.OnEntered(this, prev, Target);
			PostStateChange(prev);
        }

        public virtual void ChangeState<P>(State state, P parameter) where P : IStateParameter<T>
        {
			var prev = CurrentState;
			PreStateChange(CurrentState, state);
			CurrentState = state;
			DeliverComponents(CurrentState);
			state.OnEntered(this, prev, Target, parameter);
			PostStateChange(prev);
        }
        public virtual void ChangeState<S>() where S : State, new()
		{
			if (AutoStateCache == null) AutoStateCache = new Dictionary<Type, State>();
			if (!AutoStateCache.ContainsKey(typeof(S))) AutoStateCache.Add(typeof(S), new S());
			ChangeState(AutoStateCache[typeof(S)]);
		}

        public virtual void ChangeState<S>(IStateParameter<T> parameter) where S : State, new()
		{
			if (AutoStateCache == null) AutoStateCache = new Dictionary<Type, State>();
			if (!AutoStateCache.ContainsKey(typeof(S))) AutoStateCache.Add(typeof(S), new S());
			ChangeState(AutoStateCache[typeof(S)], parameter);
		}

		protected virtual void DeliverComponents (State state)
		{
			if (state is IComponentUser cu)
			{
                Type stateType = state.GetType();
                if (Attribute.GetCustomAttribute(stateType, typeof(DisableAutoComponents)) == null)
				{
					foreach (var prop in stateType.GetProperties())
					{
						if (prop.GetCustomAttributes(typeof(AutoComponentAttribute), false) == null) continue;
                        Type propType = prop.DeclaringType;
                        if (Components.TryGetValue(propType, out var comp))
						{
							prop.SetValue(state, comp);
							cu.OnComponentSupplied(propType, comp);
						}
					}
				}
				else
				{
					foreach (var kvp in Components)
						cu.OnComponentSupplied(kvp.Key, kvp.Value);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void PreStateChange (State fromState, State toState)
		{
			ChangingState = true;
			if (DebugOutput != null) WriteDebug("A StateMachine<{0}> is switching from {1} to {2}.", Target.GetType().Name, fromState?.GetType()?.Name, toState.GetType().Name);
			fromState?.OnLeaving(this, toState, Target);
			OnStateChanging?.Invoke(fromState, toState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void PostStateChange (State fromState)
		{
			if (DebugOutput != null) WriteDebug("A StateMachine<{0}> has switched from {1} to {2}.", Target.GetType().Name, fromState?.GetType()?.Name, CurrentState.GetType().Name);
			OnStateChanged?.Invoke(fromState, CurrentState);
			ChangingState = false;
		}

		public virtual void Update ()
		{
			if (!AllowUpdate) return;
			if (Target == null) throw new System.NullReferenceException("Target is null.");
			if (CurrentState == null) throw new System.NullReferenceException("CurrentState is null. Did you set a state after instantiate this controller?");
			if (CurrentState.AllowUpdate) CurrentState.Update(this, Target);
		}

        public virtual bool InvokeEvent(IStateEvent<T> stateEvent)
        {
			if (!stateEvent.CanInvoke(CurrentState)) return false;
			if (DebugOutput != null && CurrentState != null) WriteDebug("A StateMachine<{0}> is invoking {1}, the active state (receiver) is {2}", Target.GetType().Name, CurrentState?.GetType()?.Name, stateEvent.GetType().Name);
			CurrentState?.ReceiveEvent(this, stateEvent, Target);
			return true;
        }

		protected virtual void WriteDebug (string format, object arg0 = null, object arg1 = null, object arg2 = null)
		{
			if (DebugStringBuilder == null) DebugStringBuilder = new StringBuilder();
			DebugStringBuilder.AppendFormat(format, arg0, arg1, arg2);
			DebugOutput(DebugStringBuilder.ToString());
			DebugStringBuilder.Clear();
		}


	}
}
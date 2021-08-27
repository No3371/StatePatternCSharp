using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace BAStudio.StatePattern
{
    public class FixedStateMachine<ID, T> : IFixedStateMachine<ID, T> where ID : System.Enum
    {
		public event System.Action<string> DebugOutput;
        public T Target { get; protected set; }
        public ID CurrentStateID { get; protected set; }
        public IState<T> CurrentState { get => StateIndex.ContainsKey(CurrentStateID)? StateIndex[CurrentStateID] : null; }
        public IReadOnlyDictionary<ID, IState<T>> StateIndex { get; protected set; }
        private FixedStateMachine(T target, ReadOnlyDictionary<ID, IState<T>> index)
        {
            Target = target;
            StateIndex = index;
        }
        public bool AllowUpdate { get; set; }
        public bool WillPassEvent { get; protected set; }
        public bool ChangingState { get; protected set; }
        public event Action<ID, ID> OnStateIDChanging, OnStateIDChanged;
        public virtual void ChangeState(ID state)
        {
			var prevId = CurrentStateID;
			PreStateChange(state, state);
			CurrentStateID = state;
			CurrentState?.OnEntered(this);
			PostStateChange(prevId);
        }
        public virtual void ChangeState<P>(ID state, P parameter) where P : IStateParameter<T>
        {
			var prevId = CurrentStateID;
			PreStateChange(CurrentStateID, state);
			CurrentStateID = state;
			if (CurrentState is IParameterConsumer<T, P> pc) pc?.OnEntered(parameter);
			else CurrentState?.OnEntered(this);
			PostStateChange(prevId);
        }


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void PreStateChange (ID fromState, ID toState)
		{
			WillPassEvent = false;
			ChangingState = true;
			DebugOutput?.Invoke("StateMachine<" + Target.GetType().Name + "> is switching to: " + toState.GetType().Name);
			StateIndex[fromState]?.OnLeaving(this);
			OnStateIDChanging(fromState, toState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void PostStateChange (ID fromState)
		{
			OnStateIDChanged(fromState, CurrentStateID);
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

        public ref struct Builder
        {
            Dictionary<ID, IState<T>> index;
            public void Init ()
            {
                index = new Dictionary<ID, IState<T>>();
            }

            public void Map<S> (ID id) where S : IState<T>, new()
            {
                var s = new S();
                index.Add(id, s);
            }
            public FixedStateMachine<ID, T> Build (T target)
            {
                return new FixedStateMachine<ID, T>(target, new ReadOnlyDictionary<ID, IState<T>>(index));
            }
        }
    }

}
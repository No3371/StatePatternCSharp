using System.Collections.Generic;

namespace BAStudio.StatePattern
{
    public abstract class State<T> : IState<T>
    {
        public IStateMachine<T> StateMachine { get; }
		protected T Context { get { return StateMachine.Target; }}
        public bool AllowUpdate { get; protected set; }
		public virtual void ChangeState (IState<T> nextState)
		{
			StateMachine.ChangeState(nextState);
		}
        public abstract void OnEntered();
        public abstract void OnEntered(IStateParameter<T> parameter);
        public abstract void Update();
        public abstract void OnLeaving();
        public abstract void ReceiveEvent(IStateEvent<T> stateEvent);
    }

    public abstract class State<T, M> : State<T>, IState<T, M> where M : IStateMachine<T>
    {
        M IState<T, M>.StateMachine => (M) StateMachine;
    }
}
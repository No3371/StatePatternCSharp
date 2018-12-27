using System.Collections.Generic;

namespace BA_Studio.StatePattern
{

    public abstract class State<T> : IState<T> where T : class
    {
		public State (StateMachine<T> machine)
		{
			this.StateMachine = machine;
		}

		public T Context { get { return StateMachine.Target; }}

        public StateMachine<T> StateMachine { get; private set; }

		protected bool allowUpdate = true;

        public virtual bool AllowUpdate { get { return allowUpdate; } private set { allowUpdate = value; } }

		public State<T> ActiveStrategy { get { return StateMachine.CurrentState; }}

        public virtual void OnEntered () {}

        public virtual void OnLeaving () {}

        public abstract void Update ();

		public virtual void ChangeState (State<T> strategy)
		{
			this.StateMachine.ChangeState(strategy);
		}		

    }
}
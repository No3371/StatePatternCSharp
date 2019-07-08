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

        public StateMachine<T> StateMachine { get; }

		protected bool allowUpdate = true;

        public virtual bool AllowUpdate { get { return allowUpdate; } private set { allowUpdate = value; } }

        public virtual void OnEntered () {}
        public virtual void OnEntered (StateEventData<T> data)
		{			
            switch (data.GetType())
            {
                
            }
		}

        public virtual void OnLeaving () {}

        public abstract void Update ();

		public virtual void ChangeState (State<T> nextState)
		{
			this.StateMachine.ChangeState(nextState);
		}

		System.Type[] eventListenerCache;

		public System.Type[] EventListener
		{
			get => eventListenerCache ?? (eventListenerCache = EventListenerConfig);
		}

		// To implement: { get => new System.Type[] {}; }
		public virtual System.Type[] EventListenerConfig { get => null; }

		public virtual void EventReceived (StateEventData<T> data)
		{			
            switch (data.GetType())
            {
                
            }
		}
    }
}
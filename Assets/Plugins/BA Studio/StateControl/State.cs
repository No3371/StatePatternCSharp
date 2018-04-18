

namespace BA_Studio.Lib.StateControl
{
    public interface IState<T> where T : class
	{
		StateController<T> StateController { get; }
	    bool AllowUpdate { get; }
		void Update ();
		void FixedUpdate ();
		void OnEntered ();
		void OnLeaving ();
	}

	public interface IInterState<T> where T : class
	{
		State<T> NextState { get; }
	}

    public class State<T> : IState<T> where T : class
    {
		public State (StateController<T> controller)
		{
			this.StateController = controller;
		}

		public T Target { get { return StateController.Owner; }}

        public StateController<T> StateController { get; private set; }

		protected bool allowUpdate = true;

        public virtual bool AllowUpdate { get { return allowUpdate; } private set { allowUpdate = value; } }

        public virtual void OnEntered()
        {
            
        }

        public virtual void OnLeaving()
        {
            
        }

        public virtual void Update()
        {
            throw new System.NotImplementedException(this.GetType().ToString());
        }

		public virtual void FixedUpdate ()
		{			
            throw new System.NotImplementedException();
		}

		// Reset() allows you to cache states instead of new() one whenever you need a state.
		public virtual void Reset () {}

		public virtual void ChangeState (State<T> state)
		{
			this.StateController.ChangeState(state);
		}
		
    }
}
namespace BA_Studio.StatePattern
{
    public interface IState<T> where T : class
	{
		StateMachine<T> StateMachine { get; }
	    bool AllowUpdate { get; }
		void Update ();
		void OnEntered ();
		void OnLeaving ();
	}
}
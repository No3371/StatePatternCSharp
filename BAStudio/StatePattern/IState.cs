namespace BAStudio.StatePattern
{
    public interface IState<T>
	{
		IStateMachine<T> StateMachine { get; }
	    bool AllowUpdate { get; }
		void OnEntered ();
		void Update ();
		void OnLeaving ();
		void ReceiveEvent (IStateEvent<T> stateEvent);
	}

	/// <summary>
	/// The interface is for states that specifically made for not just certain context but also certain state machine class
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="M"></typeparam>
    public interface IState<T, M> : IState<T> where M : IStateMachine<T>
	{
		new M StateMachine { get; }
	}
}
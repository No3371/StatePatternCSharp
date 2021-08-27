namespace BAStudio.StatePattern
{
    public interface IStateMachine<T>
	{
		T Target { get; }
		bool AllowUpdate { get; set; }
		bool WillPassEvent { get; }
		bool ChangingState { get; }
		bool InvokeEvent (IStateEvent<T> stateEvent);
		void Update ();
	}

}
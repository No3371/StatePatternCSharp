namespace BAStudio.StatePattern
{
    public interface IParameterConsumer<T, P> where P : IStateParameter<T>
	{
		void OnEntered (IStateParameter<T> parameter);
	}
}
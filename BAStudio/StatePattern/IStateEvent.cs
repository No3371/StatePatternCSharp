namespace BAStudio.StatePattern
{
    public interface IStateEvent<T> {
        bool CanInvoke (IState<T> currentState);
    }
}
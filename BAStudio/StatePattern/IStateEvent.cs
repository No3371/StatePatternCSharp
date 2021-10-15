namespace BAStudio.StatePattern
{
    public interface IStateEvent<T>
    {
        bool CanInvoke (StateMachine<T>.State currentState);
    }
}
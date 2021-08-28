namespace BAStudio.StatePattern
{
    public interface IStateEvent<T>
    {
        bool CanInvoke (State<T> currentState);
    }
}
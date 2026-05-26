namespace BAStudio.StatePattern
{
    public interface ICommandReceiverState<T, C, R>
    {
        R? Command(IStateMachine<T> machine, C command);
    }

    public interface ICommandReceiverState<C, R>
    {
        R? Command(C command);
    }
}
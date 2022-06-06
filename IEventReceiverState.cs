namespace BAStudio.StatePattern
{
    /// <summary>
    /// A IEventReceiverState can receive event of type E.
    /// </summary>
    /// Using two type params is a little too verbose but not that bad.
    /// <typeparam name="E">Avoid boxing/unboxing.</typeparam>
    public interface IEventReceiverState<T, E>
    {
        void ReceiveEvent(StateMachine<T> machine, T subject, E ev);
    }
}

namespace BAStudio.StatePattern
{
    public abstract class State<T>
    {
        public bool AllowUpdate { get; protected set; } = true;
        public abstract void OnEntered(StateMachine<T> machine, State<T> previous, T context);
        public abstract void Update(StateMachine<T> machine, T context);
        public abstract void OnLeaving(StateMachine<T> machine, State<T> next, T context);
        public virtual void ReceiveEvent(StateMachine<T> machine, IStateEvent<T> stateEvent, T context) {}
    }

    public sealed class NoOpState<T> : State<T>
    {
        public override void OnEntered(StateMachine<T> machine, State<T> previous, T context) {}
        public override void Update(StateMachine<T> machine, T context) {}
        public override void OnLeaving(StateMachine<T> machine, State<T> next, T context) {}
        public override void ReceiveEvent(StateMachine<T> machine, IStateEvent<T> stateEvent, T context) {}
    }
}
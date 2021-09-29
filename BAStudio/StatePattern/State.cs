
namespace BAStudio.StatePattern
{
    public abstract class State<T>
    {
        protected State()
        {
            AllowUpdate = true;
        }

        public bool AllowUpdate { get; protected set; }
        public abstract void OnEntered(StateMachine<T> machine, State<T> previous);
        public abstract void Update(StateMachine<T> machine);
        public abstract void OnLeaving(StateMachine<T> machine, State<T> next);
        public virtual void ReceiveEvent(StateMachine<T> machine, IStateEvent<T> stateEvent) {}
    }

    public sealed class NoOpState<T> : State<T>
    {
        public override void OnEntered(StateMachine<T> machine, State<T> previous) {}
        public override void Update(StateMachine<T> machine) {}
        public override void OnLeaving(StateMachine<T> machine, State<T> next) {}
        public override void ReceiveEvent(StateMachine<T> machine, IStateEvent<T> stateEvent) {}
    }
}
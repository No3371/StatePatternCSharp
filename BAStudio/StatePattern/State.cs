using System.Collections.Generic;

namespace BAStudio.StatePattern
{
    public abstract class State<T> : IState<T>
    {
        public bool AllowUpdate { get; protected set; }
        public abstract void OnEntered(IStateMachine<T> machine);
        public abstract void OnEntered(IStateMachine<T> machine, IStateParameter<T> parameter);
        public abstract void Update(IStateMachine<T> machine);
        public abstract void OnLeaving(IStateMachine<T> machine);
        public abstract void ReceiveEvent(IStateMachine<T> machine, IStateEvent<T> stateEvent);
    }
}
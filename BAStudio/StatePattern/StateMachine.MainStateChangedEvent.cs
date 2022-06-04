namespace BAStudio.StatePattern
{
    public partial class StateMachine<T>
    {
        public struct MainStateChangedEvent
        {
            public State from, to;

            public MainStateChangedEvent(State from, State to)
            {
                this.from = from;
                this.to = to;
            }
        }
    }

}
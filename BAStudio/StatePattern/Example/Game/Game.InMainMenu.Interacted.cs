namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        public partial class InMainMenu
        {
            public struct Interacted : IStateEvent<Game>
            {
                public Interaction Interaction { get; set; }
                public bool CanInvoke(StateMachine<Game>.State currentState)
                {
                    return currentState is InMainMenu;
                }
            }
        }
    }
}
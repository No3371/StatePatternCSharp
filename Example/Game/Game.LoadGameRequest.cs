namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        public struct LoadGameRequest
        {
            public int Slot { get; set; }

            public bool CanInvoke(StateMachine<Game>.State currentState)
            {
                return currentState is InMainMenu;
            }
        }
    }
}
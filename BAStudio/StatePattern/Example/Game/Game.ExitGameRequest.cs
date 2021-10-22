namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        public struct ExitGameRequest : IStateEvent<Game>, IStateParameter<Game>
        {
            public bool ToMainMenu { get; }
            public bool CanInvoke(StateMachine<Game>.State currentState) => currentState is GamePlaying || currentState is GamePaused;
        }
    }
}
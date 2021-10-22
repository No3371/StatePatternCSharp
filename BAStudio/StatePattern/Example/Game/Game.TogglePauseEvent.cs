namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        public struct TogglePauseEvent : IStateEvent<Game>
            {
                public bool CanInvoke(StateMachine<Game>.State currentState) => currentState is GamePlaying || currentState is GamePaused;
            }
    }
}
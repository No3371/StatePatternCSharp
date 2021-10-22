namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        public class GamePlaying : StateMachine<Game>.State
        {
            public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game context, IStateParameter<Game> parameter = null) {}

            public override void OnLeaving(StateMachine<Game> machine, StateMachine<Game>.State next, Game context) {}

            public override void Update(StateMachine<Game> machine, Game context) {}

            public override void ReceiveEvent(StateMachine<Game> machine, IStateEvent<Game> stateEvent, Game context)
            {
                switch (stateEvent)
                {
                    case TogglePauseEvent e:
                    {
                        machine.ChangeState<GamePaused>();
                        break;
                    }
                    case ExitGameRequest e:
                    {
                        machine.ChangeState<GameStopping>(e);
                        break;
                    }
                }
            }
        }
    }
}
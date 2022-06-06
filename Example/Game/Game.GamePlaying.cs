namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        public class GamePlaying : StateMachine<Game>.State, IEventReceiverState<Game, GameEvent>
        {
            public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game subject, object parameter = null) {}

            public override void OnLeaving(StateMachine<Game> machine, StateMachine<Game>.State next, Game subject, object parameter = null) {}

            public override void Update(StateMachine<Game> machine, Game subject) {}

            public void ReceiveEvent(StateMachine<Game> machine, Game subject, GameEvent ev)
            {
                switch (ev)
                {
                    case GameEvent.Pause:
                    {
                        machine.ChangeState<GamePaused>();
                        break;
                    }
                    case GameEvent.Exit:
                    {
                        machine.ChangeState<GameExiting>();
                        break;
                    }
                }
            }
        }
    }
}
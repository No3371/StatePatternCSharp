namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        public class GamePaused : StateMachine<Game>.State, IEventReceiverState<Game, GameEvent>
        {
            public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game context, object parameter = null)
            {
                context.timeScaleWorld = 0;
            }

            public override void OnLeaving(StateMachine<Game> machine, StateMachine<Game>.State next, Game context, object parameter = null)
            {
                context.timeScaleWorld = 1;
            }

            public override void Update(StateMachine<Game> machine, Game context) {}

            public void ReceiveEvent(StateMachine<Game> machine, Game context, GameEvent ev)
            {
                switch (ev)
                {
                    case GameEvent.Pause:
                    {
                        machine.ChangeState<GamePlaying>();
                        break;
                    }
                }
            }
        }
    }
}
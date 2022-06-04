using System.Threading.Tasks;

namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        public class NewGame : StateMachine<Game>.State
        {
            Task _newGameTask;
            public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game context, object parameter = null)
            {
                _newGameTask = NewGameTask();
                switch (parameter)
                {
                    case NewGameOptions options:
                    {
                        // if (options.hardMode) SetMoney(-9999);
                        break;
                    }
                }
            }

            public override void OnLeaving(StateMachine<Game> machine, StateMachine<Game>.State next, Game context, object parameter = null)
            {
            }

            public override void Update(StateMachine<Game> machine, Game context)
            {
                if (_newGameTask.IsCompleted) machine.ChangeState<GamePlaying>();
            }

            async Task NewGameTask () {}
        }

        struct NewGameOptions
        {
            public bool hardMode;

            public NewGameOptions(bool hardMode)
            {
                this.hardMode = hardMode;
            }
        }
    }
}
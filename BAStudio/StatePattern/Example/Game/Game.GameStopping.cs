using System.Threading.Tasks;

namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        public class GameStopping : StateMachine<Game>.State
        {
            bool _backToMainMenu;
            Task _stopGameTask;
            public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game context, IStateParameter<Game> parameter = null)
            {
                _backToMainMenu = false;
                if (parameter is ExitGameRequest request)
                {
                    _backToMainMenu = request.ToMainMenu;
                }
                else
                {
                    throw new System.ArgumentException("Expecting parameter of type ExitGameRequest but got " + parameter?.GetType());
                }

                _stopGameTask = StopGame();
            }

            public override void OnLeaving(StateMachine<Game> machine, StateMachine<Game>.State next, Game context) {}

            public override void Update(StateMachine<Game> machine, Game context)
            {
                if (_stopGameTask.IsCompleted)
                {
                    if (_backToMainMenu)
                    {
                        machine.ChangeState<InMainMenu>();
                    }
                    else
                    {
                        context.ExitGame();
                    }
                }
            }

            async Task StopGame () {}
        }
    }
}
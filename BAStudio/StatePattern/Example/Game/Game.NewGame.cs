using System.Threading.Tasks;

namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        public class NewGame : StateMachine<Game>.State
        {
            Task _newGameTask;
            public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game context, IStateParameter<Game> parameter = null)
            {
                _newGameTask = NewGameTask();
            }

            public override void OnLeaving(StateMachine<Game> machine, StateMachine<Game>.State next, Game context)
            {
            }

            public override void Update(StateMachine<Game> machine, Game context)
            {
                if (_newGameTask.IsCompleted) machine.ChangeState<GamePlaying>();
            }

            async Task NewGameTask () {}
        }
    }
}
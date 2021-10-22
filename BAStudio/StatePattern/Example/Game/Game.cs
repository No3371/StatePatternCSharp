
using System.Threading.Tasks;

namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        StateMachine<Game> _stateMachine;
        float timeScaleWorld, timeScaleUI;
        public Game()
        {
            _stateMachine = new StateMachine<Game>(this);
            _stateMachine.SetComponent<ILogger, SimpleLogger>(new SimpleLogger()); // The SimpleLogger will be provided to states implements IComponentUser interface, we'll talk about this later
            _stateMachine.ChangeState<Init>(); // Set the first state
        }

        /// <summary>
        /// Assuming it's called 60 times/s
        /// </summary>
        public void Update () => _stateMachine.Update();

        public void SetupStuff () {}

        public async Task SetupAsyncStuff () {}
        public void ExitGame () {}
    }
}
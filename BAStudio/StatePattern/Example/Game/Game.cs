
using System.Threading.Tasks;

namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        StateMachine<Game> stateMachine;
        float timeScaleWorld, timeScaleUI;
        public Game()
        {
            stateMachine = new StateMachine<Game>(this);
            stateMachine.SetComponent<ILogger, SimpleLogger>(new SimpleLogger());
        }

        /// <summary>
        /// Assuming it's called 60 times/s
        /// </summary>
        public void Update () => stateMachine.Update();

        public void SetupStuff () {}

        public async Task SetupAsyncStuff () {}
        public void ExitGame () {}
    }
}
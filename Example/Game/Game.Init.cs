
using System;
using System.Threading.Tasks;

namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        public class Init : StateMachine<Game>.State, IComponentUser
        {
            [AutoComponent] ILogger Logger { get; }
            Task _setupTask;
            public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game subject, object parameter = null)
            {
                subject.SetupStuff();
                _setupTask = subject.SetupAsyncStuff();
            }

            public override void OnLeaving(StateMachine<Game> machine, StateMachine<Game>.State next, Game subject, object parameter = null) {}

            public override void Update(StateMachine<Game> machine, Game subject)
            {
                if (_setupTask.IsCompleted)
                {
                    Logger.Log("Init completed!");
                    machine.ChangeState<InMainMenu>();
                }
            }

            public void OnComponentSupplied(Type t, object o) {}
        }
    }
}
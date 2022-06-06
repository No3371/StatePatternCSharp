using System;
using System.Threading.Tasks;

namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        public class LoadGame : StateMachine<Game>.State, IComponentUser
        {
            [AutoComponent] ILogger Logger { get; }
            Task _loadGameTask;
            public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game subject, object parameter = null)
            {
                if (parameter is LoadGameRequest r)
                    _loadGameTask = LoadGameTask(r.Slot);
                else
                    throw new System.ArgumentException("Expecting parameter of type LoadGameRequest but got a " + parameter.GetType());
            }

            public override void OnLeaving(StateMachine<Game> machine, StateMachine<Game>.State next, Game subject, object parameter = null) {}

            public override void Update(StateMachine<Game> machine, Game subject)
            {
                if (_loadGameTask.IsCompleted) machine.ChangeState<GamePlaying>();
            }

            async Task LoadGameTask (int saveSlot)
            {
                Logger.Log("Loading saveslot#{0}", saveSlot);
            }

            public void OnComponentSupplied(Type t, object o) {}
        }
    }
}
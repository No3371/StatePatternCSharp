using System;

namespace BAStudio.StatePattern.Example
{
    public class MovementMachine : StateMachine<Movement>
    {
        public MovementMachine(Movement target) : base(target) {}

        public class ExampleState : State
        {
            public override void OnEntered(StateMachine<Movement> machine, State previous, Movement subject, object parameter = null)
            {
                throw new NotImplementedException();
            }

            public override void OnLeaving(StateMachine<Movement> machine, State next, Movement subject, object parameter = null)
            {
                throw new NotImplementedException();
            }

            public override void Update(StateMachine<Movement> machine, Movement subject)
            {
                throw new NotImplementedException();
            }
        }
    }

}
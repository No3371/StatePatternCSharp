namespace BAStudio.StatePattern.Example
{
    public abstract partial class Movement
    {
        public class Grounded : StateMachine<Movement>.State
        {
            public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement subject, object parameter = null)
            {
                subject.Velocity.SetY(0);
            }

            public override void OnLeaving(StateMachine<Movement> machine, StateMachine<Movement>.State next, Movement subject, object parameter = null) {}

            public override void Update(StateMachine<Movement> machine, Movement subject)
            {
                subject.GroundCheck();
                if (subject.Velocity.y < 0)
                {
                    machine.ChangeState<Falling>();
                    return;
                }
                if (subject.CurrentInput.Jump)
                {
                    machine.ChangeState<Jumping>();
                    return;
                }
            }
        }
    }

}
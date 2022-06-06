namespace BAStudio.StatePattern.Example
{
    public abstract partial class Movement
    {
        public class Falling : StateMachine<Movement>.State
        {
            bool isDoubleJumped;
            public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement subject, object parameter = null)
            {
                isDoubleJumped = previous is Jumping && parameter is bool b == true;
            }

            public override void OnLeaving(StateMachine<Movement> machine, StateMachine<Movement>.State next, Movement subject, object parameter = null) {}

            public override void Update(StateMachine<Movement> machine, Movement subject)
            {
                subject.ApplyGravity();

                if (subject.GroundCheck())
                {
                    machine.ChangeState<Grounded>();
                    return;
                }

                if (!isDoubleJumped && subject.CurrentInput.Jump)
                {
                    machine.ChangeState<Jumping>(); // Jump again
                    return;
                }
            }
        }
    }

}
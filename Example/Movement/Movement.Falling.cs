namespace BAStudio.StatePattern.Example
{
    public abstract partial class Movement
    {
        public class Falling : StateMachine<Movement>.State
        {
            bool isDoubleJumped;
            public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement context, object parameter = null)
            {
                isDoubleJumped = previous is Jumping && parameter is bool b == true;
            }

            public override void OnLeaving(StateMachine<Movement> machine, StateMachine<Movement>.State next, Movement context, object parameter = null) {}

            public override void Update(StateMachine<Movement> machine, Movement context)
            {
                context.ApplyGravity();

                if (context.GroundCheck())
                {
                    machine.ChangeState<Grounded>();
                    return;
                }

                if (!isDoubleJumped && context.CurrentInput.Jump)
                {
                    machine.ChangeState<Jumping>(); // Jump again
                    return;
                }
            }
        }
    }

}
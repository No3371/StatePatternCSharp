namespace BAStudio.StatePattern.Example
{
    public abstract partial class Movement
    {
        public class Jumping : StateMachine<Movement>.State
        {
            bool isDoubleJumping;
            public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement context, object parameter = null)
            {
                isDoubleJumping = previous is Jumping;
                switch (parameter)
                {
                    case JumpParameter jp:
                    {
                        context.Velocity += new Vector3(0, 1000, 0) * jp.JumpMultiplier;
                        break;
                    }
                    case null:
                    {
                        context.Velocity += new Vector3(0, 1000, 0);
                        break;
                    }
                }
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

                if (!isDoubleJumping && context.CurrentInput.Jump)
                {
                    machine.ChangeState<Jumping>(); // Jump again
                    return;
                }

                if (context.Velocity.y < 0)
                {
                    machine.ChangeState<Falling>(isDoubleJumping);
                    return;
                }
            }
        }
    }

}
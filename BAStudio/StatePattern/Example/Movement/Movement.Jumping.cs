namespace BAStudio.StatePattern.Example
{
    public abstract partial class Movement
    {
        public class Jumping : StateMachine<Movement>.State
        {
            public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement context, object parameter = null)
            {
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
                context.Velocity -= new Vector3(0, 9.8f, 0);
                context.GroundCheck();
                if (context.IsGrounded)
                {
                    machine.ChangeState<Grounded>();
                    return;
                }
                if (context.Velocity.y < 0)
                {
                    machine.ChangeState<Falling>();
                    return;
                }
            }
        }
    }

}
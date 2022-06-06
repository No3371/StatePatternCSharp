namespace BAStudio.StatePattern.Example
{
    public abstract partial class Movement
    {
        public class Jumping : StateMachine<Movement>.State
        {
            bool isDoubleJumping;
            public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement subject, object parameter = null)
            {
                isDoubleJumping = previous is Jumping;
                switch (parameter)
                {
                    case JumpParameter jp:
                    {
                        subject.Velocity += new Vector3(0, 1000, 0) * jp.JumpMultiplier;
                        break;
                    }
                    case null:
                    {
                        subject.Velocity += new Vector3(0, 1000, 0);
                        break;
                    }
                }
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

                if (!isDoubleJumping && subject.CurrentInput.Jump)
                {
                    machine.ChangeState<Jumping>(); // Jump again
                    return;
                }

                if (subject.Velocity.y < 0)
                {
                    machine.ChangeState<Falling>(isDoubleJumping);
                    return;
                }
            }
        }
    }

}
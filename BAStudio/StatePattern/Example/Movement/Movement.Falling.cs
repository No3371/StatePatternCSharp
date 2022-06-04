namespace BAStudio.StatePattern.Example
{
    public abstract partial class Movement
    {
        public class Falling : StateMachine<Movement>.State
        {
            public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement context, object parameter = null) {}

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
            }
        }
    }

}
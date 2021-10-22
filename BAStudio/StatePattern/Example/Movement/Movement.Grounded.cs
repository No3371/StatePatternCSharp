namespace BAStudio.StatePattern.Example
{
    public abstract partial class Movement
    {
        public class Grounded : StateMachine<Movement>.State
        {
            public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement context, IStateParameter<Movement> parameter = null)
            {
                context.Velocity.SetY(0);
            }

            public override void OnLeaving(StateMachine<Movement> machine, StateMachine<Movement>.State next, Movement context) {}

            public override void Update(StateMachine<Movement> machine, Movement context)
            {
                context.GroundCheck();
                if (context.Velocity.y < 0)
                {
                    machine.ChangeState<Falling>();
                    return;
                }
                if (context.CurrentInput.Jump)
                {
                    machine.ChangeState<Jumping>();
                    return;
                }
            }
        }
    }

}
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BAStudio.StatePattern;

namespace BAStudio.StatePattern.Example
{
    public class MovementMachine : StateMachine<Movement>
    {
        public MovementMachine(Movement target) : base(target) {}

        public class ExampleState : State
        {
            public override void OnEntered(StateMachine<Movement> machine, State previous, Movement context, IStateParameter<Movement> parameter = null)
            {
                throw new NotImplementedException();
            }

            public override void OnLeaving(StateMachine<Movement> machine, State next, Movement context)
            {
                throw new NotImplementedException();
            }

            public override void Update(StateMachine<Movement> machine, Movement context)
            {
                throw new NotImplementedException();
            }
        }
    }

    public abstract class Movement
    {
        public Movement ()
        {
            stateMachine = new StateMachine<Movement>(this);
            stateMachine.ChangeState<Grounded>();
        }

        StateMachine<Movement> stateMachine;

        Input CurrentInput { get; set; }
        bool IsGrounded { get; set; }
        Vector3 Velocity { get; set; }
        public abstract void GroundCheck ();
        void Update () // Called regularly
        {
            stateMachine.Update();
        }
        
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
                }
                
            }
        }

        public class Falling : StateMachine<Movement>.State
        {
            public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement context, IStateParameter<Movement> parameter = null) {}

            public override void OnLeaving(StateMachine<Movement> machine, StateMachine<Movement>.State next, Movement context) {}

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

        public class Jumping : StateMachine<Movement>.State
        {
            public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement context, IStateParameter<Movement> parameter = null)
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

            public override void OnLeaving(StateMachine<Movement> machine, StateMachine<Movement>.State next, Movement context) {}

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

        struct JumpParameter : IStateParameter<Movement>
        {
            public float JumpMultiplier { get; set; }
        }
    }

}
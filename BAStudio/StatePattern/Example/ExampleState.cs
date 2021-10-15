using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BAStudio.StatePattern;

namespace statepatterncsharp.BAStudio.StatePattern.Example
{

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
        
        public class Grounded : State<Movement>
        {
            public override void OnEntered(StateMachine<Movement> machine, State<Movement> previous, Movement context)
            {
                context.Velocity.SetY(0);
            }

            public override void OnLeaving(StateMachine<Movement> machine, State<Movement> next, Movement context) {}

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

        public class Falling : State<Movement>
        {
            public override void OnEntered(StateMachine<Movement> machine, State<Movement> previous, Movement context) {}

            public override void OnLeaving(StateMachine<Movement> machine, State<Movement> next, Movement context) {}

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

        public class Jumping : State<Movement>
        {
            public override void OnEntered(StateMachine<Movement> machine, State<Movement> previous, Movement context)
            {
                context.Velocity += new Vector3(0, 1000, 0);
            }

            public override void OnLeaving(StateMachine<Movement> machine, State<Movement> next, Movement context) {}

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
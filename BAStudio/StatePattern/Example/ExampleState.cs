using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BAStudio.StatePattern;

namespace statepatterncsharp.BAStudio.StatePattern.Example
{
    public class ExampleCustomMachine<T> : StateMachine<T>
    {
        public ExampleCustomMachine(T target) : base(target) {}
    }

    public class MovementMachine : StateMachine<Movement>
    {
        public MovementMachine(Movement target) : base(target) {}
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
            public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement context)
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
            public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement context) {}

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
            public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement context)
            {
                context.Velocity += new Vector3(0, 1000, 0);
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
    }

}
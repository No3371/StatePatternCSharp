using System.Collections.Generic;
using System.Runtime.Serialization;
using BAStudio.StatePattern;

namespace BAStudio.StatePattern.Example
{
    public abstract partial class Movement
    {
        public Movement ()
        {
            _stateMachine = new StateMachine<Movement>(this);
            _stateMachine.ChangeState<Grounded>();
        }

        StateMachine<Movement> _stateMachine;

        Input CurrentInput { get; set; }
        bool IsGrounded { get; set; }
        Vector3 Velocity { get; set; }
        public abstract void GroundCheck ();
        void Update () // Called regularly
        {
            _stateMachine.Update();
        }
    }

}
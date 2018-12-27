using BA_Studio.StatePattern;

public class State_InAir : State<Player>
{
    public State_InAir (StateMachine<Player> machine) : base(machine) {}

    public override void Update ()
    {
        // If a key is pressed when still jumping in the air, let's jump one more time;
        if (Input.GetKeyDown (KeyCode.Space)) ChangeState(new State_Jumping(stateMachine));
        
        //-------------------------------------------------------//
        // Some GroundCheck code to check if we touch the ground //
        //-------------------------------------------------------//
        if (GroundCheck()) ChangeState(new State_Grounded(stateMachine));
    }
}

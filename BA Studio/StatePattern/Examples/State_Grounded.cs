using BA_Studio.StatePattern;

public class State_Grounded : State<Player>
{
    public State_Grounded (StateMachine<Player> machine) : base(machine) {}

    public override void Update ()
    {
        // If a key is pressed when still jumping in the air, let's jump one more time;
        if (Input.GetKeyDown (KeyCode.Space)) ChangeState(new State_Jumping(stateMachine));
        
        // if (Input.GetKey (KeyCode.A)) MoveLeft(); 
        // if (Input.GetKey (KeyCode.D)) MoveRight();
        //...
        // Some pseudo movement code so the player can move, only when grounded.
    }
}
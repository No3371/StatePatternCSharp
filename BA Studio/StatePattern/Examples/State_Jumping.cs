using BA_Studio.StatePattern;

public class State_Jumping : State<Player>
{
    public State_Jumping (StateMachine<Player> machine) : base(machine) {}

    public override void OnEntered ()
    {
        //------------------------------------------------//
        // Some jumping code to push the player object up //
        //------------------------------------------------//
        ChangeState(new State_InAir(stateMachine));
    }
}

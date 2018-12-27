using BA_Studio.StatePattern;
public class Player : MonoBehaviour
{
    StateMachine<Player> stateMachine;
    
    // Awake() in UnityEngine is called when the GameObject is loaded, which is the player object in this case.
    void Awake ()
    {
        stateMachine = new StateMachine<Player>(this); //Initialize the machine.
        stateMachine.ChangeState(new State_Grounded(stateMachine)); // Assume the player will stand on ground from the beginning.
    }

    // Update() in UnityEngine is called every frame.
    void Update ()
    {
        stateMachine?.Update(); 
    }
}
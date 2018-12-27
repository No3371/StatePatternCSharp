# StatePatternC#
A simple framework does nothing on its own σ (´・ω・`), made to help you keep your code structure simple.  
It's quite useful in case you need a subject to behave differently in different sitautions, especially you want it to transform swiftly between different logic.


## Concept
To pull out the actual worker logic from a class and put in different States, kind of shape the original object into a data storage / handle.

## Usage
The framework is composed of 2 parts: **StateMachine<T>** and **State<T>**.  
Let's see a *pseudo* Unity Engine C# **Character Movement** controller for example.  
Imagine we want a movement system that a character can jump multiple times, we usually have to keep track of last jump time and do some if check lying here and there in the script, which is messy.  

Let's follow the framework to apply state pattern, below is the minimum main script would look alike you need to have the system working.

```c#
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
        stateMachine?.Update(); // You have to keep the state machine updated! It only do its job when updated.
    }
}
```
  
  
  
Instead of doing all the condition check and perform the logic in this script, we instead create a state machine to manage states for us, and let the states do the job.

Let's see what does the **State_Grounded** looks like:
##### Grounded State  
```c#
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
```

It's really simple, right? But even my grandma can achieve this level of work, with just 1 script... Let's add another state so we can jump.

##### Jumping State  
```c#
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
    
    public override void Update () {}
}
```

Well, I know, I know... the big comment block makes me looks really lazy, but I believe you are not here to learn how to move a player... and I'm indeed lazy (ﾟ∀ﾟ)

Anyway, as you can see, this time we use *OnEntered()* to make the player jump.

*OnEntered()* , only get called when the state is entered/switched to by the State Machine,and its friend, *OnLeaving()*, only get called before the State Mahcine is going to another state.

Quite useful when you need some 1 time effect instead of *Update()*.  
*p.s. However, the frequency of Update() definitely depends on how you use the framework.*

We gotta be a down-to-earth person, so it's time for the InAir state, the InAir represent a situation that the player object is still in air, not yet fall to the ground.

##### InAir State  
```c#
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
```

Now that's basically done for a neat double/multiple jump system.
🥂

## Shared Data
What if you want to store some data only to be used by states? I understand, sometimes I also want to keep my code extremely clean so that everything exposed in a degree that except for those who need it, nothing can reference it.

You can store any data into a *StateMachine* by SetSharedObject<T>(). For example, I want the player can only jump 2 times in a row, so I do this:


##### Jumping State  
```c#
using BA_Studio.StatePattern;

public class State_Jumping : State<Player>
{
    public State_Jumping (StateMachine<Player> machine) : base(machine) {}

    public override void OnEntered ()
    {
        //------------------------------------------------//
        // Some jumping code to push the player object up //
        //------------------------------------------------//
        SetSharedObject<int>("Jumped", GetSharedObject<int>("Jump") + 1); // <-----------
        ChangeState(new State_InAir(stateMachine));
    }
    
    public override void Update () {}
}
```

We insert a jumped times check here.
##### InAir State  
```c#
using BA_Studio.StatePattern;

public class State_InAir : State<Player>
{
    public State_InAir (StateMachine<Player> machine) : base(machine) {}

    public override void Update ()
    {
        // If a key is pressed when still jumping in the air, let's jump one more time;
        if (Input.GetKeyDown (KeyCode.Space) && GetSharedObject<int>("Jumped") < 2) ChangeState(new State_Jumping(stateMachine)); // <------
        
        //-------------------------------------------------------//
        // Some GroundCheck code to check if we touch the ground //
        //-------------------------------------------------------//
        if (GroundCheck()) ChangeState(new State_Grounded(stateMachine));
    }
}
```

Then we add a piece of code to reset the jump times to 0 when grouned.
##### Grounded State  
```c#
using BA_Studio.StatePattern;

public class State_Grounded : State<Player>
{
    public State_Grounded (StateMachine<Player> machine) : base(machine) {}
    
    public override void OnEntered ()
    {
        SetSharedObject<int>("Jumped", 0);  // <-----
    }

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
```

By this, we add a jump times limit feature without touching the main Player script.
🥂

## Context & Target
Context and Target indicate same thing: the original script controlled by the state machine, to the states, it's Context; to the state machine, it's the Target.

## Debug The Transitions
To see the state transitition, simply subscribe to the string delegate StateMachine<T>.debugLogOutput.
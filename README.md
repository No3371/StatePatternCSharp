# StatePatternC#
A simple framework does nothing on its own σ (´・ω・`), made to help you keep your code readable.
It's quite useful in case you need a subject to behave differently in different sitautions, especially you want it to switch swiftly between different logic.
The concept is pulling out the actual worker logic from a class and put in different States, transform the original object into a data storage / handle.

## Features
- Easily create, edit, debug system behavior without messing with loads of code
- Use Parameter and Event to easily integrate/communicate with other parts of the program
- Component System (Dependency Injection) to make it even better

## Overview
- class **StateMachine<T>**: It manage states of its assigned subject which is of type T.
- class **StateMachine<T>.State**: A piece of code defining how the subject should behave.
- interface **IStateParameter<T>**: Objects of a type implements this can be passed when changing state.
- interface **IStateEvent<T>**: Objects of a type implements this can be passed anytime to the active state.
- interface **IComponentUser<T>**: States implements this supports Dependency Injection.

## Getting Started

### Create a subject

Consider we are making a game, and now we want to implement the main game flow with this framework. We start with a `Game` class:

```csharp
public partial class Game
{
    StateMachine<Game> _stateMachine;
    public Game()
    {
        _stateMachine = new StateMachine<Game>(this);
        _stateMachine.SetComponent<ILogger, SimpleLogger>(new SimpleLogger()); // The SimpleLogger is available to all states, we'll talk about this later
        _stateMachine.ChangeState<Init>(); // Set the first state
    }

    public void Update () => _stateMachine.Update();
}
```

The StateMachine<T> is like a plugin to any class. Very few changes are needed to use this framework, even if it's not a fresh new class, we can attach StateMachine to it to apply state pattern.

Now let's create the first state, `Init`:

```csharp
public class Init : StateMachine<Game>.State
{
    Task _setupTask;
    public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game context, IStateParameter<Game> parameter = null)
    {
        context.SetupStuff();
        _setupTask = context.SetupAsyncStuff();
    }

    public override void OnLeaving(StateMachine<Game> machine, StateMachine<Game>.State next, Game context) {}

    public override void Update(StateMachine<Game> machine, Game context)
    {
        if (_setupTask.IsCompleted)
        {
            machine.ChangeState<InMainMenu>();
        }
    }

    public void OnComponentSupplied(Type t, object o) {}
}
```

A state is a child class of StateMachine<T>.State, there are 3 methods must be implemented:

- `OnEntered` is executed whenever the StateMachine switched the active state to it.
- `Update` is executed whenever the StateMachine's Update() get successfully called.
- `OnLeaving` is executed whenever the StateMachine switched from it to other state.





## FAQ
- The abstract methods in States are so annoything! How would I type all those for every state?

    I'm sorry about that, that's the result of a lot of design decisions, I personally use IDE (VSCode + Omnisharp) to populate stuff in less then a second so it's not a deal IMO.

- Why States are sub classes of StateMachines?

    To overcome generic definition cycle problem.


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

We gotta be a down-to-earth person, so it's time for the InAir state, so we will not remain the jump forever...

the InAir represent a situation that the player object is still in air, not yet fall to the ground.

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
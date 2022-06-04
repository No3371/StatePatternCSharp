# StatePatternC#
Another state pattern framework carefully crafted for writing better code.
The concept is pulling out the actual worker logic from a class and split the code into different States, transform the original object into a data storage / handle.

Note: this implements a Passive StateMachine, it does nothing on its own and needs to be ticked by user code.

## Features
- Easily creates, edits, debugs object behaviors without messing with unrelated code
- Make use of external parameter/events object to easily integrate/communicate with other parts of the program
- Component System (Dependency Injection) to make it even better
- Elegant design for the best develper experience

## Overview
- class **StateMachine<T>**: It manage states of its assigned subject which is of type T.
- class **StateMachine<T>.State**: The base class of all states, derive to create behaviour of your subject T.
- interface **IComponentUser<T>**: States implements this supports Dependency Injection.
- interface **IEventReceiverState<T, E>**: States implement this can receiver events of type E from StateMachine.

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
        _stateMachine.SetComponent<ILogger, SimpleLogger>(new SimpleLogger());
        _stateMachine.ChangeState<Init>(); // Set the first state
    }

    /// <summary>
    /// Assuming this get called 60 times/s
    /// </summary>
    public void Update () => _stateMachine.Update();

    // These methods are for later examples
    public void SetupStuff () {}
    public async Task SetupAsyncStuff () {}
    public void ExitGame () {}
}
```

The framework does not require your subject T to inherit something. Instead, you new a `StateMachine<T>` somewhere then you are good. Declare the StateMachine within your subject T is recommended. Very small change to a existed subject class is needed to use this framework, even if it's not a fresh new class, we can attach a StateMachine to it to start applying state pattern.

There are several intresting points in the above codes:
- It's `partial`, this is because the state classes are declared as `Game`'s subclasses and split to different files.
    - This is recommended that the states can access private members of the subject.
- There's a `SetComponent` statement. This is the Dependency Injection feature; we will talk about this later.
- We `ChangeState` immediately after the machine is newed.
    - There are 2 versions of `ChangeState`: generic/instanced.
    - In the example code generic version is used, the Init is automatically newed and cached by the machine.

### Create a state
Now let's take a look at `Init` state:

```csharp
public partial class Game
{
    public class Init : StateMachine<Game>.State
    {
        Task _setupTask; // Just for example 
        public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game context, object parameter = null)
        {
            context.SetupStuff();
            _setupTask = context.SetupAsyncStuff();
        }

        public override void OnLeaving(StateMachine<Game> machine, StateMachine<Game>.State next, Game context, object parameter = null) {}

        public override void Update(StateMachine<Game> machine, Game context)
        {
            if (_setupTask.IsCompleted)
            {
                Logger.Log("Init completed!");
                machine.ChangeState<InMainMenu>();
            }
        }
    }
}
```

A state is a child class of StateMachine<T>.State, there are 3 methods must be implemented:

- `OnEntered` is executed whenever the StateMachine switched the active state to it.
- `Update` is executed whenever the StateMachine's Update() get successfully called.
- `OnLeaving` is executed whenever the StateMachine switched away from it to other state.

The method signatures are long and complex, this is how it is after several attempts to refactor across several years. As long as you utilize your IDE and not hand-typing everything it should not be a problem.

For `Init` state, we do everything is required to initialize the game, Assuming the sync `SetupStuff()` and an async `SetupAsyncStuff()` are all we need, we keep the state updated and `ChangeState<InMainMenu>` when we know that we are good to go.

In the `ChangeState<InMainMenu>` call, the machine will call `Init.OnLeaving` then `InMainMenu.OnEntered`.

Noted that there's no return for these method. For `Update()`, this means we can have statements after  a `ChangeState()` call, but always return after `ChangeState()` call should makes it easier to maintain.

That's all. Well, at least for basic usage of the framework, you are good to go. Keep reading if you are interested in complete features  some practical examples.

### Parameters

All `ChangeState` methods takes an optional `object parameter`, it will be passed along to both the from/to states. This allows states to know more about a transition.

The parameter is of type `object`, this means anything can be passed. For example, pass an Exception to a Game.Exiting state, we can add some code to upload the error stack trace to out server.

The recommended practice is to switch on the object:

```csharp
public class NewGame : StateMachine<Game>.State
{
    public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game context, object parameter = null)
    {
        switch (parameter)
        {
            case NewGameOptions options:
            {
                if (options.hardMode) SetMoney(-9999);
                break;
            }
        }
    }
    ...
}

struct NewGameOptions
{
    public bool hardMode;

    public NewGameOptions(bool hardMode)
    {
        this.hardMode = hardMode;
    }
}
```

### Events

`StateMachine<T>` provides a `SendEvent<E>(E ev)` method that will send the ev to all active states implemented with `IEventReceiverState<T, E>`.

The `IEventReceiverState<T, E>` has an type parameter E, this means a State must implement `ReceiveEvent()` for every E it's interested in. This is easier to maintain and can avoid boxing/unboxing with `object`.

There are also some pre-built events used by the framework itself, users can implement the interface for these:
- `InternalSignal`: an Enum of simple signals like MachinePaused and MachineResumed.
- `PopupStateStarted`: Contains reference to the PopupState.
- `PopupStateEnded`: Contains reference to the PopupState.
- `MainStateChanged`: Contains references to the from/to States.
- `SideTrackStateChanged`: Contains which track it is and the references to the from/to States. (MultiTrackStateMachine)

```csharp
public class InMainMenu : StateMachine<Game>.State,
                          IEventReceiverState<Game, LoadGameRequest>,
                          IEventReceiverState<Game, InMainMenu.Interaction>
{
    ...
    public void ReceiveEvent(StateMachine<Game> machine, Game context, LoadGameRequest ev)
    {
        machine.ChangeState<LoadGame>(ev);
    }

    public void ReceiveEvent(StateMachine<Game> machine, Game context, InMainMenu.Interaction ev)
    {
        switch (ev)
        {
            case Interaction.NewGame:
                machine.ChangeState<NewGame>();
                break;
            case Interaction.Load:
                // LOAD GAME LOGIC
                break;
            case Interaction.Exit:
                context.ExitGame();
                break;
        }
    }

    public enum Interaction
    {
        NewGame,
        Load,
        Exit
    }
}
```

### Dependency Injection

```csharp
public partial class Game
{
    public class Init : StateMachine<Game>.State, IComponentUser
    {
        [AutoComponent] ILogger Logger { get; set; }
        ...
        public void OnComponentSupplied(Type t, object o) {}
    }
}
```

States implements `IComponentuser` interface are delivered the components provided through `StateMachine.SetComponent()`.

This happens when:
- For `ChangeState(State)`, Components Delivering always happens to the next state everytime.
- For `ChangeState<S>()`, there's a configuration flag will decide the behaviour.
    - (Noted that this generic version use internally newed/cached states)
    - If `InjectionOnCachedStateOnlyNew` is set to true, For every S, Components Delivering happens only at the first time `ChangeState<S>()` is called. Otherwise it also happens everytime `ChangeState<S>()` is called.

What exact does Components Delivering do?

The framework provides auto fill-in functionality and that's what makes it "Dependency Injection" (I don't know, is it?)

By default, all setter properties (even private ones) marked with `[AutoComponent]` will be filled in, given a component of matching type is provided. After a successful fill-in, `OnComponentSupplied` is called once for that one component filled-in.

You can mark a state with `[DisableAutoComponents]` and it will disable the auto fill-in for that state, instead, it simply calls `OnComponentSupplied` for every components provided and leave the rest to you.

If an `IComponentuser` state is not marked with `[DisableAutoComponents]` but no `[AutoComponent]` is found, it's treated like it's marked with `[DisableAutoComponents]`.

As you may have noticed, in this example it is an `ILogger` while in the first example code snippet, it's `SetComponent<ILogger, SimpleLogger>`, and the `[AutoComponent]` here is an `ILogger`. So you know that it allows some polymorphism here.
- The signature is `SetComponent<PT, CT> (CT obj) where CT : PT`; as long as PT **isAssignableFrom** CT it'd work. For example, `SimpleLogger` implements `ILogger` so it works, if `SimpleLogger` is derived from `BaseLogger`, then `SetComponent<BaseLogger, SimpleLogger>` will works for `[AutoComponent] BaseLogger logger { get; set; }`.

### PopupStates

PopupStates are like fire-and-forget states, there's no state transition. There's no limit on how many & what type of PopupStates is active at same time.

It's a good for fit for something related to the your subject, but is simple/transient. For example, debuffs in RPG games.

At some degree it also allows multi states at same time. But if you really needs that, consider trying out `MultiTrackStateMachine` which has "SideTracks" alongside the main State.

(It's inspired by... Moodlets, from Sims4, when something happens your sim gets a mood; A mood is of some type of emotion and has a reason; The mood ends after some time or due to something happens.)


## FAQ
- The abstract methods in States are so annoything! How would I type all those for every state?

    This is actually the simplest yet usable interface I found. I personally use IDE (VSCode + Omnisharp) to populate stuff in less then a second so it's not a deal IMO.

- Why States are sub classes of StateMachines?

    The framework is built around generic, I explored the design several times and there's no better option for this framework until I find a better solution of nested generic parameters.

## Examples

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
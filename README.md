# StatePatternC#

(👷 ATM this repo contains an `EnumExtension` class which is only for Unity, but can be easily changed for .NET by **replacing all `UnsafeUtility.SizeOf` with `Unsafe.SizeOf`**.)

Another state pattern framework crafted carefully for writing better code.
The concept is to pull out the actual worker logic from a class and split the code into different States, transforming the original object into a data storage/handle.

Note: this implements a Passive StateMachine, it does nothing on its own and needs to be ticked by user code.

## Features
- Easily creates, edits, and debugs object behaviors without messing with unrelated code
- Make use of external parameter/events object to easily integrate/communicate with other parts of the program
- Component System (Dependency Injection) to make it even better
- Elegant design for the best developer experience

## Overview
- class **StateMachine<T>**: It manages the states of its assigned subject which is of type T.
- class **StateMachine<T>.State**: The base class of all states.
- interface **IComponentUser<T>**: States implement this supports Dependency Injection.
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

The framework does not require your subject T to inherit something. Instead, just new a `StateMachine<T>` somewhere then you are good. Declare the StateMachine within your subject T is recommended. Not much change to existed subject class is needed to use this framework, even if it's not a fresh new class, we can attach a StateMachine to it to start applying state pattern.

There are several interesting points in the above codes:
- It's `partial`, this is because the state classes are declared as `Game`'s subclasses and split into different files.
    - This is recommended that the states can access private members of the subject.
- There's a `SetComponent` statement. This is the Dependency Injection feature; we will talk about this later.
- We `ChangeState` immediately after the machine is newed.
    - There are 2 versions of `ChangeState`: generic/instanced.
    - In the example code generic version is used, and the Init is automatically newed and cached by the machine.

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

A state is a child class of StateMachine<T>.State, there are 3 methods that must be implemented:

- `OnEntered` is executed whenever the StateMachine switched the active state to it.
- `Update` is executed whenever the StateMachine's Update() get successfully called.
- `OnLeaving` is executed whenever the StateMachine switched away from it to another state.

The method signatures are long and complex, this is how it is after several attempts to refactor across several years. As long as you utilize your IDE and do not hand-typing everything it should not be a problem.

For `Init` state, we do everything required to initialize the game, Assuming the sync `SetupStuff()` and an async `SetupAsyncStuff()` are all we need, we keep the state updated and `ChangeState<InMainMenu>` when we know that we are good to go.

In the `ChangeState<InMainMenu>` call, the machine will call `Init.OnLeaving` then `InMainMenu.OnEntered`.

Noted that there's no return for these methods. For `Update()`, this means we can have statements after a `ChangeState()` call, but always returning after `ChangeState()` call should make it easier to maintain.

That's all. Well, at least for basic usage of the framework, you are good to go. Keep reading if you are interested in all features and some practical examples.

### Parameters

Both `ChangeState()` implementations take an optional `object parameter`, which will be passed along to both the from/to states. This allows states to know more about a transition.

The parameter is of type `object`, this means anything can be passed. For example, pass an Exception to a Game.Exiting state, we can add some code to upload the error stack trace to remote servers.

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

`StateMachine<T>` provides a `SendEvent<E>(E ev)` method that will send the `ev` to all active states implemented with `IEventReceiverState<T, E>`.

The `IEventReceiverState<T, E>` has a type parameter E, this means a State must implement `ReceiveEvent()` for every E it's interested in. This is easier to maintain and can avoid boxing/unboxing with `object`.

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
- For `ChangeState(State)`, Components Delivering always happens every time.
- For `ChangeState<S>()`, there's a configuration flag that will decide the behavior.
    - (Noted that this generic version use internally newed/cached states)
    - If `InjectionOnCachedStateOnlyNew` is set to true, For every S, Components Delivering happens only at the first time `ChangeState<S>()` is called. Otherwise, it also happens every time `ChangeState<S>()` is called.

What exactly does Components Delivering do?

The framework provides auto fill-in functionality and that's what makes it "Dependency Injection" (I don't know, is it?)

By default, all setter properties (even private ones) marked with `[AutoComponent]` will be filled in, given a component of matching type is provided. After a successful fill-in, `OnComponentSupplied` is called once for that one component filled in.

You can mark a state with `[DisableAutoComponents]` and it will disable the auto fill-in for that state, instead, it simply calls `OnComponentSupplied` for every component provided and leave the rest to you.

If an `IComponentuser` state is not marked with `[DisableAutoComponents]` but no `[AutoComponent]` is found, it's treated like it's marked with `[DisableAutoComponents]`.

As you may have noticed, in this example it is an `ILogger` while in the first example code snippet, it's `SetComponent<ILogger, SimpleLogger>`, and the `[AutoComponent]` here is an `ILogger`. So you know that it allows some polymorphism here.
- The signature is `SetComponent<PT, CT> (CT obj) where CT : PT`; as long as PT **isAssignableFrom** CT it'd work. For example, `SimpleLogger` implements `ILogger` so it works, if `SimpleLogger` is derived from `BaseLogger`, then `SetComponent<BaseLogger, SimpleLogger>` will works for `[AutoComponent] BaseLogger logger { get; set; }`.

### PopupStates

PopupStates are like fire-and-forget states, there's no state transition. There's no limit on how many & what type of PopupStates is active at the same time.

It's a good fit for something related to the subject but is simple/transient. For example, debuffs in RPG games.

To some degree, it also allows multi states at the same time. But if you really need that, consider trying out `MultiTrackStateMachine` which has "SideTracks" alongside the main State.

(It's inspired by... Moodlets, from Sims4, when something happens your sim gets a mood; A mood is of some type of emotion and has a reason; The mood ends after some time or due to something happens.)

### Debug The Transitions
To log state transitions, subscribe to the string delegate `StateMachine<T>.DebugOutput`.

## FAQ
- The abstract methods in States are so annoying! How would I type all those for every state?

    This is actually the simplest yet most usable interface I figured. I personally use IDE (VSCode + Omnisharp) to populate stuff in less than a second so it's not a deal IMO.

- Why States are sub-classes of StateMachines?

    The framework is built around generic, I explored the design several times and there's no better option for this framework until I find a better solution of nested generic parameters.

- What is Context? What is Target?

    Context and Target indicate the same thing: the subject controlled by the state machine, to the states, it's Context; to the state machine, it's the Target.

- Is `StateMachine` thread-safe?

    It's not. At the moment I believe the best multi-threaded usage will be like every StateMachine only gets updated by 1 thread (One thread to many machines). 

## Examples

### Double Jumping

This example shows how to easily create a double-jump behavior with only 3 states.

```csharp
public class Jumping : StateMachine<Movement>.State
{
    bool isDoubleJumping;
    public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement context, object parameter = null)
    {
        isDoubleJumping = previous is Jumping;
        switch (parameter)
        {
            case JumpParameter jp:
            {
                context.Velocity += new Vector3(0, 1000, 0) * jp.JumpMultiplier;
                break;
            }
            case null:
            {
                context.Velocity += new Vector3(0, 1000, 0);
                break;
            }
        }
    }
    ...
    public override void Update(StateMachine<Movement> machine, Movement context)
    {
        context.ApplyGravity();

        if (context.GroundCheck())
        {
            machine.ChangeState<Grounded>();
            return;
        }

        if (context.CurrentInput.Jump)
        {
            machine.ChangeState<Jumping>(); // Jump again
            return;
        }

        if (context.Velocity.y < 0)
        {
            machine.ChangeState<Falling>();
            return;
        }
    }
}

public class Grounded : StateMachine<Movement>.State
{
    public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement context, object parameter = null)
    {
        context.Velocity.SetY(0);
    }
    ...
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
            return;
        }
    }
}

public class Falling : StateMachine<Movement>.State
{
    bool isDoubleJumped;
    public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement context, object parameter = null)
    {
        isDoubleJumped = previous is Jumping && parameter is bool b == true;
    }
    ...
    public override void Update(StateMachine<Movement> machine, Movement context)
    {
        context.ApplyGravity();

        if (context.GroundCheck())
        {
            machine.ChangeState<Grounded>();
            return;
        }

        if (!isDoubleJumped && context.CurrentInput.Jump)
        {
            machine.ChangeState<Jumping>(); // Jump again
            return;
        }
    }
}
```
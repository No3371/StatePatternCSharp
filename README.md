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

Let's see some examples before learning anything about the framework, it may be confusing but the code should be clear enough to be understandable.

## Quick Demo: Double Jumping

Here is a simple example shows how to easily create a double-jump behavior with only 3 states: `Jumping`, `Falling`, `Grounded`.

Jump button can be pressed anytime but it will only jumps when it's grounded or it jumped only once.

Triple or more jumps can also be implmented by keeping an int variable for how many times it jumps between 2 Grounded.

The jumping logic is built into the `Jumping` state, whenever it enters the state, a Jump is performed (if allowed).

With this setup, the main Movement class is very clean and contains no jumping behavior code.

```csharp
// Jumping indicates the character is jumping up & y position is increasing, actual jumping is executed in OnEntered()
// If any ground found below the character, goes go Grounded
// If no double-jumping is performed and jump button is pressed, goes to Jumping again
// If it reached the highest point and started falling down due to gravity, goes to Falling
public class Jumping : StateMachine<Movement>.State
{
    bool isDoubleJumping;
    public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement subject, object parameter = null)
    {
        isDoubleJumping = previous is Jumping;
        switch (parameter)
        {
            case JumpParameter jp: // Allow variable jump height, ex: hold to jump higher
            {
                subject.Velocity += new Vector3(0, 1000, 0) * jp.JumpMultiplier;
                break;
            }
            case null:
            {
                subject.Velocity += new Vector3(0, 1000, 0);
                break;
            }
        }
    }
    ...
    public override void Update(StateMachine<Movement> machine, Movement subject)
    {
        subject.ApplyGravity();

        if (subject.GroundCheck())
        {
            machine.ChangeState<Grounded>();
            return;
        }

        if (!isDoubleJumping && subject.CurrentInput.Jump)
        {
            machine.ChangeState<Jumping>(); // Jump again
            return;
        }

        if (subject.Velocity.y < 0)
        {
            machine.ChangeState<Falling>();
            return;
        }
    }
}

// Grounded indicates the character is steadily grounded now
// If somehow the ground below disappear, start falling
// And if jump button pressed, start jumping
public class Grounded : StateMachine<Movement>.State
{
    public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement subject, object parameter = null)
    {
        subject.Velocity.SetY(0);
    }
    ...
    public override void Update(StateMachine<Movement> machine, Movement subject)
    {
        subject.GroundCheck();
        if (subject.Velocity.y < 0)
        {
            machine.ChangeState<Falling>();
            return;
        }
        if (subject.CurrentInput.Jump)
        {
            machine.ChangeState<Jumping>();
            return;
        }
    }
}

// Falling indicates the character is falling down & y position is decreasing
// If there's a ground below the character, goes to Grounded
// Or goes to Jumping again if jump button pressed and no double-jump is performed
public class Falling : StateMachine<Movement>.State
{
    bool isDoubleJumped;
    public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement subject, object parameter = null)
    {
        isDoubleJumped = previous is Jumping && parameter is bool b == true;
    }
    ...
    public override void Update(StateMachine<Movement> machine, Movement subject)
    {
        subject.ApplyGravity();

        if (subject.GroundCheck())
        {
            machine.ChangeState<Grounded>();
            return;
        }

        if (!isDoubleJumped && subject.CurrentInput.Jump)
        {
            machine.ChangeState<Jumping>(); // Jump again
            return;
        }
    }
}
```

## Quick Demo: UI Menu Interaction

This exmaple shows how external code can communicate with states through events.

```csharp
public enum MainMenuButton
{
    NewGame,
    Continue,
    Options
}

public class MainMenu : StateMachine<GameCore>.State, IEventReceiverState<GameCore, MainMenuButton>
{
    ...
    public void ReceiveEvent(StateMachine<GameCore> machine, GameCore subject, MainMenuButton ev)
    {
        switch (ev)
        {
            case MainMenuButton.NewGame:
                break;
            case MainMenuButton.Continue:
                break;
            case MainMenuButton.Options:
                break;
        }
    }
}


public class MainMenuController : MonoBehaviour
{
    GameCore gameCore;
    public void OnButton_NewGame () // Called by UI
    {
        gameCore.StateMachine.SendEvent(GameCore.MainMenuButton.NewGame);
    }

    public void OnButton_Continue () // Called by UI
    {
        gameCore.StateMachine.SendEvent(GameCore.MainMenuButton.Continue);
    }
    public void OnButton_Options () // Called by UI
    {
        gameCore.StateMachine.SendEvent(GameCore.MainMenuButton.Options);
    }
}

```

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
    /// Assuming this get called 60 times/s.
    /// </summary>
    public void Update () => _stateMachine.Update();

    // These methods are for examples below
    public void SetupStuff () {}
    public async Task SetupAsyncStuff () {}
    public void ExitGame () {}
}
```

The framework does not require your subject T to inherit something. Instead, just new a `StateMachine<T>` somewhere then you are good (declaring it within your subject T is recommended).

For existed subject classes, not much change to existed code is required, the StateMachine and States only requires a reference to the subject to be functional.

There are several interesting points in the above codes:
- It's `partial`, this is because the state classes are declared as `Game`'s subclasses and splitted into different files.
    - This is recommended so the states can access private members of the subject.
- There's a `SetComponent()` statement. This is the Dependency Injection feature; we will talk about this later.
- We `ChangeState` immediately after the machine is newed.
    - `ChangeState()` can be called with generic parameter or a state instance.
    - In the example code the generic implementation is called. This results in a Init automatically newed and cached by the machine.

### Create a state
Now let's take a look at `Init` state:

```csharp
public partial class Game
{
    public class Init : StateMachine<Game>.State
    {
        Task _setupTask; // Just for example 
        public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game subject, object parameter = null)
        {
            subject.SetupStuff();
            _setupTask = subject.SetupAsyncStuff();
        }

        public override void OnLeaving(StateMachine<Game> machine, StateMachine<Game>.State next, Game subject, object parameter = null) {}

        public override void Update(StateMachine<Game> machine, Game subject)
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
    - It's not guaranteed that this will get called between `OnEntered()` and `OnLeaving()`, because these 2 methods can `ChangeState()` too.
- `OnLeaving` is executed whenever the StateMachine switched away from it to another state.

The method signatures are long and complex, this is how it is after several attempts to refactor across several years. As long as you utilize your IDE and do not hand-typing everything it should not be a problem.

In `Init` state, we do everything required to initialize the game, Assuming the sync `SetupStuff()` and an async `SetupAsyncStuff()` are all we need, we keep the state updated and `ChangeState<InMainMenu>` when we know that we are good to go.

Inside the `ChangeState<InMainMenu>` statement, the machine will call `Init.OnLeaving` then `InMainMenu.OnEntered`.

Noted that although we can have statements after a `ChangeState()` call, always returning after `ChangeState()` calls should ease the maintainence.

That's all...Well, at least for basic usage of the framework, you are good to go. Keep reading if you are interested in all features.

### Parameters

Both `ChangeState()` implementations take an optional `object parameter`, which will be passed along to both the from/to states, this allows them to know more about the transition.

The parameter is of type `object`, this means anything can be passed. For example, pass an Exception to a Game.Exiting state, we can add some code to upload the error stack trace to remote servers.

The recommended practice is to switch on the object:

```csharp
public class NewGame : StateMachine<Game>.State
{
    public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game subject, object parameter = null)
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
    public void ReceiveEvent(StateMachine<Game> machine, Game subject, LoadGameRequest ev)
    {
        machine.ChangeState<LoadGame>(ev);
    }

    public void ReceiveEvent(StateMachine<Game> machine, Game subject, InMainMenu.Interaction ev)
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
                subject.ExitGame();
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
    - If `OnlyInjectsNewForCachedStates` is set to true, For every S, Components Delivering happens only at the first time `ChangeState<S>()` is called. Otherwise, it also happens every time `ChangeState<S>()` is called.

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

    This is actually the simplest yet most usable interface I figured. I personally use IDE (VSCode + Omnisharp, QuickFix - AutoImplmenting) to one-click populate stuff so it's solvable problem in my opinion.

- Why States are sub-classes of StateMachines?

    The framework is built around generic, I explored the design several times and there's no better option for this framework until I find a better solution of nested generic parameters.

- Is `StateMachine` thread-safe?

    Not for now. At the moment I believe the best multi-threaded usage will be like every StateMachine only gets updated by 1 thread (One thread to many machines).


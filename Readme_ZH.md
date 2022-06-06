# StatePatternC#

（👷 目前此專案內的 `EnumExtension` 使用了 Unity API，但**將所有 `UnsafeUtility.SizeOf` 替換為 `Unsafe.SizeOf`** 就能在 .NET 使用）

又一個狀態機 Framework，為了幫助寫出更好的 code 而精心打造。

基本概念是將業務邏輯從 Class 中移出並拆分為不同的狀態（State） Class，讓原始 Class 變成單純的資料及參照物件。

請注意：這是一個被動狀態機（Passive StateMachine），本身不會有任何動作，需要從外部推動運作。

## 特色
- 避免在開發中面對無關程式碼，輕鬆創建、編輯物件行為並除錯
- 利用外部的事件（Event）與參數（Parameter）物件來跟外部程式碼整合溝通
- 具備 Component 機制 (Dependency Injection)
- 精心設計的介面提供最佳的開發體驗

## Overview
- class **StateMachine<T>**: 狀態機，負責管理 T 的狀態物件
- class **StateMachine<T>.State**: 所有狀態物件的基底類別
- interface **IComponentUser<T>**: 實作此介面的狀態支援 Component 機制 (Dependency Injection)
- interface **IEventReceiverState<T, E>**: 實作此介面的狀態可接受油狀態機轉發的 E 型別事件物件

## 開始使用

### 創建一個主題（Subject）

假設我們要製作遊戲，且現在要使用這個框架實作主要系統流程。讓我們從定義 `Game` 開始:

```csharp
public partial class Game
{
    StateMachine<Game> _stateMachine;
    public Game()
    {
        _stateMachine = new StateMachine<Game>(this);
        _stateMachine.SetComponent<ILogger, SimpleLogger>(new SimpleLogger());
        _stateMachine.ChangeState<Init>(); // 設定初始狀態
    }

    /// <summary>
    /// 假設以 60 fps 呼叫
    /// </summary>
    public void Update () => _stateMachine.Update();

    // 這些成員會在其他範例中被使用
    public void SetupStuff () {}
    public async Task SetupAsyncStuff () {}
    public void ExitGame () {}
}
```

此框架不需要你的主題 T 繼承自特定類別，只需要找個地方 new 出一個 `StateMachine<T>` 就好；建議選項是在主題 T 內部宣告。

使用此框架不需要對現有程式碼做出大幅改動，即使不是一個新建而空蕩的類別，也只需要新增一個 StateMachine<T> 宣告就能開始採用 StatePattern。

上述範例程式碼中，有幾點值得一提：
- 其是 `partial` 類別，這是因為 `Game` 的狀態類別是以子類別的形式宣告在其中，並被分隔到不同的檔案中`
    - 這是建議作法，因為這樣子狀態就能存取主題的私有類別
- 有一行 `SetComponent` 陳述句。這就是此框架的 Dependency Injection 功能。下方有更完整的介紹
- 在狀態機被 new 出來之後，馬上執行了 `ChangeState`
    - 此框架提供兩種 `ChangeState`： 泛型（Genric） 及狀態實例
    - 在此範例中呼叫了泛型版本。一個 Init 會被自動 new 出來並且被狀態機快取

### 創建一個狀態（State）
再來看看 `Init` 狀態的定義:

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

每個狀態（State）都衍生自 StateMachine<T>.State，必須實作三個方法：

- `OnEntered`：每當狀態機剛切換到該狀態時執行
- `Update`：每當狀態機的 `Update()` 被呼叫時執行
- `OnLeaving`：每當狀態機要從該狀態切換到其他狀態時執行

這些方法的 Signature 都又臭又長，但這已經是數次重構之後的成果了。只要善用 IDE、避免手打，這應該不是太大的問題。

我們在 `Init` 狀態中，把初始化遊戲該做的事情都做一做（範例中假設我們需要的只有同步方法 `SetupStuff()` 和非同步方法 `SetupAsyncStuff()`），等到看起來一切都完成了之後，就執行 `ChangeState<InMainMenu>`。

在這個 `ChangeState<InMainMenu>` 呼叫中，該狀態機會先執行 `Init.OnLeaving`，再來執行 `InMainMenu.OnEntered`。

需要注意的是這些方法沒有返回值。以 `Update()` 來說，不是透過返回來切換狀態代表在 `ChangeState()` 的呼叫之後執行更多陳述句是可行的，但是盡可能在每個 `ChangeState()` 之後都直接返回結束應該會比較好維護點。

就這樣。嘛，至少此框架的基本操作這樣就夠了。下面會完整介紹此框架的功能及一些比較實際的範例。

### 參數（Parameters）

此框架的兩種 `ChangeState()` 實作都接受一個選用參數 `object parameter`，這個物件會被一路傳遞到來源/目標狀態物件雙方。這讓狀態物件有機會了解狀態切換的原因。

由於接受的型別是 `object`，任何物件都可以作為參數。例如，將一個 Exception 傳遞到 Game.Exiting （即將離開遊戲）狀態，我們可以新增一些程式碼來將錯誤的 Stack Trace 上傳到伺服器。

建議作法是對該 object 做 switch：

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

`StateMachine<T>` 提供 `SendEvent<E>(E ev)` 方法，該方法會將 `ev` 轉發給所有活躍中並且有實作 `IEventReceiverState<T, E>` 的狀態物件。

`IEventReceiverState<T, E>` 介面有一個額外的泛型參數 E，也就是說，狀態類別要針對每個關心的事件類別都實作一個 `ReceiveEvent()`。這個設計較好維護且可以迴避使用 `object` 作為事件事件參數類別時發生 boxing/unboxing。

甚至框架本身也在利用這個機能！使用者可以針對下列事件類別實作介面：
- `InternalSignal`：一套簡單的 Enum 作為內部通知訊號，例如 MachinePaused（狀態機被暫停）、MachineResumed（狀態機解除暫停）.
- `PopupStateStarted`：一個 PopupState 開始運作的通知，內含指向該 PopupState 的參照。
- `PopupStateEnded`：一個 PopupState 終止的通知，內含指向該 PopupState 的參照。
- `MainStateChanged`：狀態機主要 State 已切換的通知，內含指向來源/目標 States 雙方的通知。
- `SideTrackStateChanged`：狀態機副 State 已切換的通知，內含副 State 所在通道及指向來源/目標 States 雙方的通知。 (MultiTrackStateMachine)

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
    public override void OnEntered(StateMachine<Movement> machine, StateMachine<Movement>.State previous, Movement subject, object parameter = null)
    {
        isDoubleJumping = previous is Jumping;
        switch (parameter)
        {
            case JumpParameter jp:
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

        if (subject.CurrentInput.Jump)
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
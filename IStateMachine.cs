using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BAStudio.StatePattern
{
    /// <summary>
    /// Minimal contract for driving a state machine forward each frame.
    /// Implemented by StateMachine&lt;T&gt;, wrappers, and adapters.
    /// </summary>
    public interface IStateMachine
    {
        void Update();
        bool UpdatePaused { get; }
        bool IsChangingState { get; }
        bool IsUpdating { get; }

#if UNITY_2017_1_OR_NEWER
        bool IsFixedUpdating { get; }
        bool IsLateUpdating { get; }
        void FixedUpdate();
        void LateUpdate();
#endif
    }

    /// <summary>
    /// Typed facade for state machine interaction: state transitions, popup states, events, observation, and setup.
    /// <para>States receive this interface in their lifecycle methods. For operations beyond this facade
    /// (debug), cast to <c>StateMachine&lt;T&gt;</c>.</para>
    /// </summary>
    public interface IStateMachine<T> : IStateMachine
    {
        T Subject { get; }
        IState<T> CurrentState { get; }

        void ChangeState(IState<T> state, object parameter = null);
        void ChangeState<S>(object parameter = null) where S : IState<T>;
        bool SendEvent<E>(E ev);
        bool SendEvent<S, E>(E ev, bool shouldThrow) where S : IState<T>;
        R? SendCommand<C, R>(C command, bool shouldThrow);
        void Popup(IPopupState<T> state, object parameter = null);
        Task PopupAsync(IPopupState<T> state, object parameter = null);
        S Popup<S>(object parameter = null) where S : IPopupState<T>, new();
        void EndPopupState(IPopupState<T> state, object parameter = null);
        IReadOnlyCollection<IPopupState<T>> ViewPopupStates();

        event Action<IState<T>, IState<T>> OnStateChanging;
        event Action<IState<T>, IState<T>> OnStateChanged;
        event Action<IPopupState<T>> PopupStateStarted;
        event Action<IPopupState<T>> PopupStateEnded;

        void Cache<S>(S state) where S : IState<T>;
        void SetComponent<PT, CT>(CT obj) where CT : PT;
        IStateResolver StateResolver { get; set; }
        bool DeliverOnlyOnceForCachedStates { get; set; }
    }
}

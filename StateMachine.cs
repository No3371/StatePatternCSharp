using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BAStudio.StatePattern
{
    [DisableAutoComponents]
    public partial class StateMachine<T> : IStateMachine<T>, IComponentUser
    {
        protected System.Action<string, object[]> debugOutput;
        public event System.Action<string, object[]> DebugOutput { add => debugOutput += value; remove => debugOutput -= value; }
        public int DebugFlags { get; set; } = 0;
        public const int DebugFlag_StateChange = 1 << 0;
        public const int DebugFlag_PopupState = 1 << 1;
        public const int DebugFlag_Event = 1 << 2;
        public const int DebugFlag_Component = 1 << 3;
        public const int DebugFlag_All = DebugFlag_StateChange | DebugFlag_PopupState | DebugFlag_Event | DebugFlag_Component;

        protected StateMachine()
        {
            CurrentState = new NoOpState();
        }

        public StateMachine(T subject)
        {
            Subject = subject;
            CurrentState = new NoOpState();
            UpdatePaused = false;
        }
        public T Subject { get; protected set; }
        public IState<T> CurrentState { get; protected set; }

        private bool updatePaused;
        /// <summary>
        /// <para>If false, Update() simply returns immediately.</para>
        /// <para>This is recommended over not calling Update() because states can check on this and know it's not getting Update().</para>
        /// </summary>
        public virtual bool UpdatePaused
        {
            get => updatePaused;
            set
            {
                updatePaused = value;
                SendEvent(value ? InternalSignal.MachinePaused : InternalSignal.MachineResumed);
            }
        }

        /// <summary>
        /// <para> Optimization flag.</para>
        /// <para> The generic ChangeState<S> create, cache and reuse the S. </para>
        /// <para> Enable this to skip DI if you make sure all needed components are already provided via SetComponent.</para>
        /// </summary>
        public bool DeliverOnlyOnceForCachedStates { get; set; } = false;
        public IStateResolver StateResolver { get; set; } = ActivatorStateResolver.Instance;
        public event Action<IState<T>, IState<T>> OnStateChanging;
        public event Action<IState<T>, IState<T>> OnStateChanged;
        protected Dictionary<Type, IState<T>> AutoStateCache { get; set; }
        protected List<IPopupState<T>> PopupStates { get; set; }
        protected List<IPopupState<T>> PopupStatesToEnd { get; set; }
        public event Action<IPopupState<T>> PopupStateStarted;
        public event Action<IPopupState<T>> PopupStateEnded;
        Dictionary<Type, object> Components { get; set; }
        Dictionary<Type, bool> TypesDisabledAutoComponents { get; set; }
        Dictionary<Type, PropertyInfo[]?> PropInfoMap { get; set; }
        public bool IsUpdating { get; protected set; }
        public bool IsChangingState { get => stateChangingDepth > 0; }
        int stateChangingDepth;
        public void Popup(IPopupState<T> s, object parameter = null)
        {
            if (PopupStates == null)
                PopupStates = new List<IPopupState<T>>();
            if (PopupStates.Contains(s))
                throw new Exception("PopupState already added");
            PopupStates.Add(s);
            s.OnStarting(this, parameter);
            SendEvent(new NewPopupStateEvent(s));
            PopupStateStarted?.Invoke(s);
        }

        /// <summary>
        /// The new PopupState is returned so you can do something to it like Update() once immediately.
        /// </summary>
        public S Popup<S>(object parameter = null) where S : IPopupState<T>, new()
        {
            if (PopupStates == null)
                PopupStates = new List<IPopupState<T>>();
            S s = new S();
            if (PopupStates.Contains(s))
                throw new Exception("PopupState already added");
            PopupStates.Add(s);
            s.OnStarting(this, parameter);
            SendEvent(new NewPopupStateEvent(s));
            PopupStateStarted?.Invoke(s);
            return s;
        }
        public void EndPopupState(IPopupState<T> s, object parameter = null)
        {
            if (PopupStatesToEnd == null)
                PopupStatesToEnd = new List<IPopupState<T>>();

            s.OnEnding(this, parameter);
            PopupStateEnded?.Invoke(s);
            SendEvent(new PopupStateEndedEvent(s));
            if (IsUpdating)
            {
                PopupStatesToEnd.Add(s);
                return;
            }
            PopupStates.Remove(s);
        }
        public IReadOnlyCollection<IPopupState<T>>? ViewPopupStates()
        {
            return PopupStates?.AsReadOnly();
        }

        public void SetComponent<PT, CT>(CT obj) where CT : PT
        {
            if (Components == null) Components = new Dictionary<Type, object>();
            Components[typeof(PT)] = obj;
        }

        /// <summary>
        /// Receives components from the parent machine at entry time (snapshot).
        /// Stores each component locally so the sub-machine's own states receive
        /// them through normal DeliverComponents when they enter.
        /// Virtual -- sub-machines that want isolation override to no-op.
        /// </summary>
        public virtual void OnComponentSupplied(Type componentType, object component)
        {
            if (Components == null) Components = new Dictionary<Type, object>();
            Components[componentType] = component;
        }


        /// <summary>
        /// <para>Change the state to the provide instance, with parameter supplied.</para>
        /// <para>It is recommended to use the generic version instead, internal cached states will be used.</para>
        /// <para>However, this could be useful in situations like state instances carry different data, or a non-stateful state is shared by massive amount of StateMachines.</para>
        /// </summary>
        public virtual void ChangeState(IState<T> state, object parameter = null)
        {
            PreStateChange(CurrentState, state, parameter);
            var prev = CurrentState;
            CurrentState = state;
            DeliverComponents(state); // Though maybe not useful, calling this here give prev a chance to provide components
            state.OnEntered(this, prev, parameter);
            PostStateChange(prev);

            prev?.Reset();
        }

        /// <summary>
        /// <para>Change the state to the specified type, with parameter supplied.</para>
        /// <para>The StateMachine automatically manages and keeps the state objects used.</para>
        /// </summary>
        public virtual void ChangeState<S>(object parameter = null) where S : IState<T>
        {
            if (AutoStateCache == null) AutoStateCache = new Dictionary<Type, IState<T>>();
            if (!AutoStateCache.TryGetValue(typeof(S), out var state))
            {
                state = (IState<T>)StateResolver.Resolve(typeof(S));
                AutoStateCache.Add(typeof(S), state);
                if (DeliverOnlyOnceForCachedStates) DeliverComponents(state);
            }

            var prev = CurrentState;
            PreStateChange(CurrentState, state, parameter);
            CurrentState = state;
            if (!DeliverOnlyOnceForCachedStates)
                DeliverComponents(state); // Though maybe not useful, calling this here give prev a chance to provide components
            state.OnEntered(this, prev, parameter);
            PostStateChange(prev);

            prev?.Reset();
        }

        /// <summary>
        /// If the state is an IComponentUser, this walks through all properties and try to fill in with type-matching components provided.
        /// </summary>
        /// <param name="state"></param>
        protected virtual void DeliverComponents(IState<T> state)
        {
            if (state is IComponentUser cu)
            {
                if (Components == null || Components.Count == 0) return;
                if (TypesDisabledAutoComponents == null) TypesDisabledAutoComponents = new Dictionary<Type, bool>();

                Type stateType = state.GetType();
                if (!TypesDisabledAutoComponents.TryGetValue(stateType, out var isDisabled))
                {
                    isDisabled = stateType.GetCustomAttribute<DisableAutoComponents>() != null;
                    TypesDisabledAutoComponents[stateType] = isDisabled;
                }

                if (isDisabled)
                {
                    foreach (var kvp in Components)
                        cu.OnComponentSupplied(kvp.Key, kvp.Value);
                    return;
                }

                if (PropInfoMap == null) PropInfoMap = new Dictionary<Type, PropertyInfo[]?>();

                // This state type has not been queried yet
                if (!PropInfoMap.TryGetValue(stateType, out var allPropInfos))
                {
                    var queried = stateType.GetProperties(System.Reflection.BindingFlags.Instance
                                                        | System.Reflection.BindingFlags.Public
                                                        | System.Reflection.BindingFlags.NonPublic
                                                        | BindingFlags.SetProperty)
                                                          .Where(
                                                            pi => pi.GetCustomAttribute(typeof(AutoComponentAttribute), false) != null
                                                          ).ToArray();
                    if (queried.Length == 0) PropInfoMap[stateType] = null;
                    else
                    {
                        allPropInfos = queried;
                        PropInfoMap[stateType] = allPropInfos;
                    }
                }

                if (allPropInfos == null)
                {
                    foreach (var kvp in Components)
                        cu.OnComponentSupplied(kvp.Key, kvp.Value);
                }
                else
                {
                    foreach (var pi in allPropInfos)
                    {
                        Type propType = pi.PropertyType;
                        if (Components.TryGetValue(propType, out var comp))
                        {
                            pi.SetValue(state, comp);
                            cu.OnComponentSupplied(propType, comp);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void PreStateChange(IState<T> fromState, IState<T> toState, object parameter = null)
        {
            if (debugOutput != null && (DebugFlags & DebugFlag_StateChange) != 0) LogFormat("A StateMachine<{0}> is switching from {1} to {2}.", Subject.GetType().Name, fromState?.GetType()?.Name, toState.GetType().Name);

            stateChangingDepth++;

            fromState?.OnLeaving(this, toState, parameter);
            OnStateChanging?.Invoke(fromState, toState);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void PostStateChange(IState<T> fromState)
        {
            if (debugOutput != null && (DebugFlags & DebugFlag_StateChange) != 0)
                LogFormat("A StateMachine<{0}> has switched from {1} to {2}.", Subject.GetType().Name, fromState?.GetType()?.Name, CurrentState.GetType().Name);
            SendEvent(new StateChangedEvent<T>(fromState, CurrentState));
            OnStateChanged?.Invoke(fromState, CurrentState);

            stateChangingDepth--;
            NotifyPeersOfStateChange(fromState, CurrentState);
        }

        /// <summary>
        /// Cache the provided state instance.
        /// This is useful when a state is configured with constructor and it's needed to be swapped on the fly.
        /// </summary>
        /// <param name="state"></param>
        /// <typeparam name="S"></typeparam>
        public void Cache<S>(S state) where S : IState<T>
        {
            if (DeliverOnlyOnceForCachedStates) DeliverComponents(state);
            if (AutoStateCache == null) AutoStateCache = new Dictionary<Type, IState<T>>();
            AutoStateCache[typeof(S)] = state;
        }

        /// <summary>
        /// Cache the provided state instance, keyed by its runtime type.
        /// Useful when the compile-time type is not known (e.g., auto-discovered MonoBehaviour states).
        /// </summary>
        public void CacheByType(IState<T> state)
        {
            if (AutoStateCache == null) AutoStateCache = new Dictionary<Type, IState<T>>();
            AutoStateCache[state.GetType()] = state;
            if (DeliverOnlyOnceForCachedStates) DeliverComponents(state);
        }

        public virtual void Update()
        {
            SelfDiagnosticOnUpdate();

            if (UpdatePaused) return;
            if (Subject == null) throw new System.NullReferenceException("Target is null.");

            IsUpdating = true;
            UpdateMainState();
            UpdatePopupStates();
            IsUpdating = false;
        }


#if UNITY_2017_1_OR_NEWER
        public bool IsFixedUpdating { get; protected set; }
        public bool IsLateUpdating { get; protected set; }
        public virtual void FixedUpdate()
        {
            SelfDiagnosticOnUpdate();

            if (UpdatePaused) return;
            if (Subject == null) throw new System.NullReferenceException("Target is null.");

            IsFixedUpdating = true;
            FixedUpdateMainState();
            FixedUpdatePopStates();
            IsFixedUpdating = false;
        }

        public virtual void LateUpdate()
        {
            SelfDiagnosticOnUpdate();

            if (UpdatePaused) return;
            if (Subject == null) throw new System.NullReferenceException("Target is null.");

            IsLateUpdating = true;
            LateUpdateMainState();
            LateUpdatePopStates();
            IsLateUpdating = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void FixedUpdateMainState()
        {
            if (CurrentState is not NoOpState)
            {
                if (CurrentState == null) throw new System.NullReferenceException("CurrentState is null. Did you set a state after instantiate this controller?");
                else CurrentState.FixedUpdate(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void FixedUpdatePopStates()
        {
            if (PopupStates != null)
                foreach (var ps in PopupStates) ps.FixedUpdate(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void LateUpdateMainState()
        {
            if (CurrentState is not NoOpState)
            {
                if (CurrentState == null) throw new System.NullReferenceException("CurrentState is null. Did you set a state after instantiate this controller?");
                else CurrentState.LateUpdate(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void LateUpdatePopStates()
        {
            if (PopupStates != null)
                foreach (var ps in PopupStates) ps.LateUpdate(this);
        }
#endif

        protected virtual void SelfDiagnosticOnUpdate()
        {
            if (stateChangingDepth > 0)
                throw new Exception("State change is not properly finished. Is there an exception?");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UpdateMainState()
        {
            if (CurrentState is not NoOpState)
            {
                if (CurrentState == null) throw new System.NullReferenceException("CurrentState is null. Did you set a state after instantiate this controller?");
                else CurrentState.Update(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UpdatePopupStates()
        {
            if (PopupStates == null)
                return;

            foreach (var ps in PopupStates)
                ps.Update(this);

            if (PopupStatesToEnd != null)
            {
                foreach (var ps in PopupStatesToEnd)
                    PopupStates.Remove(ps);
                PopupStatesToEnd.Clear();
            }
        }

        public virtual bool SendEvent<E>(E ev)
        {
            if (debugOutput != null && (DebugFlags & DebugFlag_Event) != 0 && CurrentState != null)
                LogFormat("A StateMachine<{0}> is sending event {1} to {2}",
                          Subject!.GetType().Name,
                          ev.GetType().Name,
                          CurrentState?.GetType()?.Name);

            SendEventToCurrentState(ev);
            SendEventToPopupStates(ev);
            SendEventToPeers(ev);

            return true;
        }

        public virtual bool SendEvent<S, E>(E ev, bool shouldThrow) where S : IState<T>
        {
            if (CurrentState is not S)
                if (shouldThrow) throw new Exception($"Event sender expected {typeof(S)}, but current state is {CurrentState.GetType().Name}.");
                else return false;

            SendEventToCurrentState(ev);
            SendEventToPopupStates(ev);
            SendEventToPeers(ev);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SendEventToCurrentState<E>(E ev)
        {
            if (CurrentState is IEventReceiverState<T, E> ers)
                ers.ReceiveEvent(this, ev);
            else if (CurrentState is IEventReceiverState<E> ers2)
                ers2.ReceiveEvent(ev);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SendEventToPopupStates<E>(E ev)
        {
            if (PopupStates != null)
                foreach (var ps in PopupStates)
                    if (ps is IEventReceiverState<T, E> ers) ers.ReceiveEvent(this, ev);
                    else if (ps is IEventReceiverState<E> ers2) ers2.ReceiveEvent(ev);
        }

        protected virtual void Log(string content)
        {
            // if (debugOutput == null) return;
            // debugOutput(content);
            debugOutput?.Invoke(content, null);
        }

        protected virtual void LogFormat(string format, params object[] args)
        {
            // if (debugOutput == null) return;
            // if (DebugStringBuilder == null) DebugStringBuilder = new StringBuilder();
            // DebugStringBuilder.AppendFormat(format, arg0);
            // debugOutput(DebugStringBuilder.ToString());
            // DebugStringBuilder.Clear();
            debugOutput?.Invoke(string.Format(format, args), args);
        }

        // protected virtual void LogFormat(string format, object arg0, object arg1)
        // {
        //     // if (debugOutput == null) return;
        //     // if (DebugStringBuilder == null) DebugStringBuilder = new StringBuilder();
        //     // DebugStringBuilder.AppendFormat(format, arg0, arg1);
        //     // debugOutput(DebugStringBuilder.ToString());
        //     // DebugStringBuilder.Clear();
        // }

        // protected virtual void LogFormat(string format, object arg0, object arg1, object arg2)
        // {
        //     // if (debugOutput == null) return;
        //     // if (DebugStringBuilder == null) DebugStringBuilder = new StringBuilder();
        //     // DebugStringBuilder.AppendFormat(format, arg0, arg1, arg2);
        //     // debugOutput(DebugStringBuilder.ToString());
        //     // DebugStringBuilder.Clear();
        // }

        // protected virtual void LogFormat(string format, object arg0, object arg1, object arg2, object arg3)
        // {
        //     // if (debugOutput == null) return;
        //     // if (DebugStringBuilder == null) DebugStringBuilder = new StringBuilder();
        //     // DebugStringBuilder.AppendFormat(format, arg0, arg1, arg2, arg3);
        //     // debugOutput(DebugStringBuilder.ToString());
        //     // DebugStringBuilder.Clear();
        // }
    }

}

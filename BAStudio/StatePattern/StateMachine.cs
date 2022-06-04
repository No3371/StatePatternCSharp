using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BAStudio.StatePattern
{
    public partial class StateMachine<T>
    {
        /// <summary>
        /// Lazily initialized.
        /// </summary>
        static StringBuilder DebugStringBuilder { get; set; }
        protected System.Action<string> debugOutput;

        public event System.Action<string> DebugOutput { add => debugOutput += value; remove => debugOutput -= value; }
        public StateMachine(T target)
        {
            Target = target;
            UpdatePaused = false;
        }
        public T Target { get; }
        public State CurrentState { get; protected set; }

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
				SendEvent(value? InternalSignal.MachinePaused : InternalSignal.MachineResumed);
			}
		}

        /// <summary>
        /// <para> Optimization flag.</para>
        /// <para> ChangeState<S> new and cache a State the firstime the State is used.</para>
        /// <para> If for every S, you set all the component S needs before the first time you call `ChangeState<S>`, 
        /// enable this to skip injection(Reflection) everytime `ChangeState<S>` is called. (not for ChangeState(State)).</para>
        /// </summary>
        public bool InjectionOnCachedStateOnlyNew { get; set; } = false;
        public event Action<State, State> OnStateChanging;
        public event Action<State, State> OnStateChanged;
        protected Dictionary<Type, State> AutoStateCache { get; set; }
        protected List<IPopupState> PopupStates { get; set; }
        public event Action<IPopupState> PopupStateStarted;
        public event Action<IPopupState> PopupStateEnded;
        Dictionary<Type, object> Components { get; set; }
        Dictionary<Type, PropertyInfo[]> PropInfoMap { get; set; }
        public void Popup(IPopupState s, object parameter = null)
        {
            PopupStates.Add(s);
            s.OnStarting(this, Target, parameter);
			SendEvent(new NewPopupStateEvent(s));
            PopupStateStarted?.Invoke(s);
        }

        /// <summary>
        /// The new PopupState is returned so you can do something to it like Update() once immediately.
        /// </summary>
        public S Popup<S>(object parameter = null) where S : IPopupState, new()
        {
            S s = new S();
            PopupStates.Add(s);
            s.OnStarting(this, Target, parameter);
			SendEvent(new NewPopupStateEvent(s));
            PopupStateStarted?.Invoke(s);
            return s;
        }
        public void EndPopupState(IPopupState s, object parameter = null)
        {
            s.OnEnding(this, Target, parameter);
            PopupStates.Remove(s);
            PopupStateEnded?.Invoke(s);
			SendEvent(new PopupStateEndedEvent(s));
        }
        public void SetComponent<PT, CT>(CT obj) where CT : PT
        {
            if (Components == null) Components = new Dictionary<Type, object>();
            Components[typeof(PT)] = obj;
        }

        /// <summary>
        /// <para>Change the state to the provide instance, with parameter supplied.</para>
        /// <para>It is recommended to use the generic version instead, internal cached states will be used.</para>
        /// <para>However, this could be useful in situations like state instances carry different data, or a non-stateful state is shared by massive amount of StateMachines.</para>
        /// </summary>
        public virtual void ChangeState(State state, object parameter = null)
        {
            var prev = CurrentState;
            PreStateChange(CurrentState, state, parameter);
            CurrentState = state;
            DeliverComponents(state); // Though maybe not useful, calling this here give prev a chance to provide components
            state.OnEntered(this, prev, Target, parameter);
            PostStateChange(prev);
        }

        /// <summary>
        /// <para>Change the state to the specified type, with parameter supplied.</para>
        /// <para>The StateMachine automatically manages and keeps the state objects used.</para>
        /// </summary>
        public virtual void ChangeState<S>(object parameter = null) where S : State, new()
        {
            if (AutoStateCache == null) AutoStateCache = new Dictionary<Type, State>();
            if (!AutoStateCache.ContainsKey(typeof(S)))
            {
                S newS = new S();
                AutoStateCache.Add(typeof(S), newS);
                if (InjectionOnCachedStateOnlyNew) DeliverComponents(newS);
            }

            var prev = CurrentState;
            var state = AutoStateCache[typeof(S)];
            PreStateChange(CurrentState, state, parameter);
            CurrentState = state;
            if (!InjectionOnCachedStateOnlyNew) DeliverComponents(state); // Though maybe not useful, calling this here give prev a chance to provide components
            state.OnEntered(this, prev, Target, parameter);
            PostStateChange(prev);
        }

        /// <summary>
        /// If the state is an IComponentUser, this walks through all properties and try to fill in with type-matching components provided.
        /// </summary>
        /// <param name="state"></param>
        protected virtual void DeliverComponents(State state)
        {
            if (state is IComponentUser cu)
            {
                Type stateType = state.GetType();
                if (PropInfoMap == null) PropInfoMap = new Dictionary<Type, PropertyInfo[]>();
                if (!PropInfoMap.TryGetValue(stateType, out var allPIs))
                {
                    // if (stateType.GetCustomAttribute<AtuoFill>)
                    var queried = stateType.GetProperties(System.Reflection.BindingFlags.Instance
                                                        | System.Reflection.BindingFlags.Public
                                                        | System.Reflection.BindingFlags.NonPublic
                                                        | BindingFlags.SetProperty)
                                                          .Where(
                                                            pi => pi.GetCustomAttribute(typeof(AutoComponentAttribute), false) != null
                                                          ).ToArray();
                    if (queried.Length == 0) PropInfoMap[stateType] = null;
                    else PropInfoMap[stateType] = queried;
                }
                else if (allPIs == null) // the state is marked to not auto fill or it has no autocomponent
                {
                    foreach (var kvp in Components)
                        cu.OnComponentSupplied(kvp.Key, kvp.Value);
                }
                else
                {
                    foreach (var pi in PropInfoMap[stateType])
                    {
                        Type propType = pi.DeclaringType;
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
        protected virtual void PreStateChange(State fromState, State toState, object parameter = null)
        {
            if (debugOutput != null) LogFormat("A StateMachine<{0}> is switching from {1} to {2}.", Target.GetType().Name, fromState?.GetType()?.Name, toState.GetType().Name);
            fromState?.OnLeaving(this, toState, Target, parameter);
            OnStateChanging?.Invoke(fromState, toState);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void PostStateChange(State fromState)
        {
            if (debugOutput != null) LogFormat("A StateMachine<{0}> has switched from {1} to {2}.", Target.GetType().Name, fromState?.GetType()?.Name, CurrentState.GetType().Name);
			SendEvent(new MainStateChangedEvent(fromState, CurrentState));
            OnStateChanged?.Invoke(fromState, CurrentState);
        }

        public virtual void Update()
        {
            if (UpdatePaused) return;
            if (Target == null) throw new System.NullReferenceException("Target is null.");

            UpdateMainState();
            UpdatePopStates();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UpdateMainState()
        {
            if (CurrentState is not NoOpState)
            {
                if (CurrentState == null) throw new System.NullReferenceException("CurrentState is null. Did you set a state after instantiate this controller?");
                else CurrentState.Update(this, Target);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UpdatePopStates()
        {
            if (PopupStates != null)
                foreach (var ps in PopupStates) ps.Update(this, Target);
        }

        public virtual bool SendEvent<E>(E ev)
        {
            if (debugOutput != null && CurrentState != null) LogFormat("A StateMachine<{0}> is invoking {1}, the active state (receiver) is {2}", Target.GetType().Name, CurrentState?.GetType()?.Name, ev.GetType().Name);

            SendEventToCurrentState(ev);
            SendEventToPopupStates(ev);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SendEventToCurrentState<E>(E ev)
        {
            if (CurrentState is IEventReceiverState<T, E> ers)
                ers.ReceiveEvent(this, Target, ev);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SendEventToPopupStates<E>(E ev)
        {
            if (PopupStates != null)
                foreach (var ps in PopupStates)
                    if (ps is IEventReceiverState<T, E> ers) ers.ReceiveEvent(this, Target, ev);
        }

        protected virtual void Log(string content)
        {
            if (debugOutput == null) return;
            debugOutput(content);
        }

        protected virtual void LogFormat(string format, object arg0)
        {
            if (debugOutput == null) return;
            if (DebugStringBuilder == null) DebugStringBuilder = new StringBuilder();
            DebugStringBuilder.AppendFormat(format, arg0);
            debugOutput(DebugStringBuilder.ToString());
            DebugStringBuilder.Clear();
        }

        protected virtual void LogFormat(string format, object arg0, object arg1)
        {
            if (debugOutput == null) return;
            if (DebugStringBuilder == null) DebugStringBuilder = new StringBuilder();
            DebugStringBuilder.AppendFormat(format, arg0, arg1);
            debugOutput(DebugStringBuilder.ToString());
            DebugStringBuilder.Clear();
        }

        protected virtual void LogFormat(string format, object arg0, object arg1, object arg2)
        {
            if (debugOutput == null) return;
            if (DebugStringBuilder == null) DebugStringBuilder = new StringBuilder();
            DebugStringBuilder.AppendFormat(format, arg0, arg1, arg2);
            debugOutput(DebugStringBuilder.ToString());
            DebugStringBuilder.Clear();
        }

        protected virtual void LogFormat(string format, object arg0, object arg1, object arg2, object arg3)
        {
            if (debugOutput == null) return;
            if (DebugStringBuilder == null) DebugStringBuilder = new StringBuilder();
            DebugStringBuilder.AppendFormat(format, arg0, arg1, arg2, arg3);
            debugOutput(DebugStringBuilder.ToString());
            DebugStringBuilder.Clear();
        }
    }

}
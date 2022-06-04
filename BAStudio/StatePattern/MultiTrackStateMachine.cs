using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BAStudio.StatePattern
{
    /// <summary>
    /// SideTracks get updated by order.
    /// SideTrack get notified when main state changed.
    /// SideTracks receives events as well.
    /// AutoStateCache is not shared bewteen tracks.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TRACK">Must be ~int32.</typeparam>
    public partial class MultiTrackStateMachine<T, TRACK> : StateMachine<T> where TRACK : unmanaged, System.Enum
    {
        public MultiTrackStateMachine(T target) : base(target)
        {
			if (System.Enum.GetUnderlyingType(typeof(TRACK)) != typeof(int))
				throw new ArgumentOutOfRangeException("The underlying type of the Enum must be int");
			var minMax = EnumExtension.MinMaxInt<TRACK>();
			if (minMax.min < 0) throw new ArgumentOutOfRangeException("The first value of the enum must be bigger then zero");
			if (minMax.max > 8) LogFormat("The TRACK enum has a max underlying value of {0}, internally a State[{0}] is being created, are you sure about this?", minMax.max);
			SideTracks = new State[minMax.max + 1];
        }

		public State[] SideTracks { get; protected set; }
        public event Action<TRACK, State, State> OnSideTrackStateChanging;
        public event Action<TRACK, State, State> OnSideTrackStateChanged;
        protected Dictionary<(TRACK, Type), State> AutoSideTrackStateCache { get; set; }

		/// <summary>
		/// <para>Change the state to the provide instance, with parameter supplied.</para>
		/// <para>It is recommended to use the generic version instead.</para>
		/// <para>However, this could be useful in situations like state instances carry different data, or a non-stateful state is shared by massive amount of StateMachines.</para>
		/// </summary>
        public virtual void ChangeSideTrackState(TRACK track, State state, object parameter = null)
        {
            int tId = track.AsInt32();
            var prev = SideTracks[tId];
			PreSideTrackStateChange(prev, state, track);
			SideTracks[tId] = state;
			DeliverComponents(state);
			state.OnEntered(this, prev, Target, parameter);
			PostSideTrackStateChange(prev, state, track);
        }

		/// <summary>
		/// <para>Change the state to the specified type, with parameter supplied.</para>
		/// <para>The StateMachine automatically manages and keeps the state objects used.</para>
		/// </summary>
        public virtual void ChangeSideTrackState<S>(TRACK track, object parameter = null) where S : State, new()
		{
			if (AutoSideTrackStateCache == null) AutoSideTrackStateCache = new Dictionary<(TRACK, Type), State>();
            (TRACK track, Type) key = (track, typeof(S));
            if (!AutoSideTrackStateCache.ContainsKey(key)) AutoSideTrackStateCache.Add(key, new S());
			ChangeSideTrackState(track, AutoSideTrackStateCache[key], parameter);
		}

		protected virtual void PreSideTrackStateChange (State fromState, State toState, TRACK sideTrack)
		{
			if (debugOutput != null) LogFormat("A StateMachine<{0}> is switching SIDETRACK#{3} from {1} to {2}.", Target.GetType().Name, fromState?.GetType()?.Name, toState.GetType().Name, sideTrack);
			fromState?.OnLeaving(this, toState, Target);
			OnSideTrackStateChanging?.Invoke(sideTrack, fromState, toState);
		}

		protected virtual void PostSideTrackStateChange (State fromState, State toState, TRACK sideTrack)
		{
			if (debugOutput != null) LogFormat("A MultiTrackStateMachine<{0}> has switched SIDETRACK#{3} from {1} to {2}.", Target.GetType().Name, fromState?.GetType()?.Name, toState.GetType().Name, sideTrack);
			SendEvent(new SideTrackStateChangedEvent(sideTrack, fromState, toState));
			OnSideTrackStateChanged?.Invoke(sideTrack, fromState, toState);
		}
		
		public override void Update ()
		{
			if (UpdatePaused) return;
			if (Target == null) throw new System.NullReferenceException("Target is null.");

			UpdateMainState();
			UpdateSideTracks();
			UpdatePopStates();
		}

		protected void UpdateSideTracks ()
		{
			for (int i = 0; i < SideTracks.Length; i++)
			if (SideTracks[i] is not NoOpState)
			{
				if (SideTracks[i] == null) throw new System.NullReferenceException("SideTrack is null. Did you set a state after instantiate this controller?");
				else SideTracks[i].Update(this, Target);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void SendEventToSideTracks <E> (E ev)
		{
			for (int i = 0; i < SideTracks.Length; i++)
				if (SideTracks[i] is IEventReceiverState<T, E> ers) ers.ReceiveEvent(this, Target, ev);
		}

        public override bool SendEvent<E> (E ev)
        {
			LogFormat("A MultiTrackStateMachine<{0}> is invoking {1}, the active state (receiver) is {2}", Target.GetType().Name, CurrentState?.GetType()?.Name, ev.GetType().Name);

			SendEventToCurrentState(ev);
			SendEventToSideTracks(ev);
			SendEventToPopupStates(ev);

			return true;
        }
    }
}
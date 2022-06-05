namespace BAStudio.StatePattern
{
    public partial class MultiTrackStateMachine<T, TRACK> where TRACK : unmanaged, System.Enum
    {
        public struct SideTrackStateChangedEvent
        {
			public TRACK track;
            public State from, to;
            public SideTrackStateChangedEvent(TRACK track, State from, State to)
            {
                this.track = track;
                this.from = from;
                this.to = to;
            }
        }
    }
}
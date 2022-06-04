namespace BAStudio.StatePattern
{
    public partial class StateMachine<T>
    {
        public struct NewPopupStateEvent
		{
			public IPopupState popupState;

            public NewPopupStateEvent(IPopupState popupState)
            {
                this.popupState = popupState;
            }
        }
    }

}
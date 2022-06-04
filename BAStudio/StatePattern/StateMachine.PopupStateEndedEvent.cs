namespace BAStudio.StatePattern
{
    public partial class StateMachine<T>
    {
        public struct PopupStateEndedEvent
        {
            public IPopupState popupState;

            public PopupStateEndedEvent(IPopupState popupState)
            {
                this.popupState = popupState;
            }
        }
    }

}
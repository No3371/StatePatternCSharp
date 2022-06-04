namespace BAStudio.StatePattern
{
    public partial class StateMachine<T>
    {
        /// <summary>
        /// PopupStates are specialized to be works on its own. It ends itself and does not make trnsition to other states.
        /// PopupStates are kept in a list so theorically unlimited ones can be added.
        /// PopupStates are not cached and will be broadcasted by events; it serves as result data of itself.
        /// </summary>
        public interface IPopupState
        {
            void OnStarting(StateMachine<T> machine, T context, object parameter = null);
            void OnEnding(StateMachine<T> machine, T context, object parameter = null);
            void Update(StateMachine<T> machine, T context);
        }
    }
}
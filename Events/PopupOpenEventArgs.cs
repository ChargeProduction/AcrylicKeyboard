namespace AcrylicKeyboard.Events
{
    public class PopupOpenEventArgs
    {
        private bool preventOpening = false;

        /// <summary>
        /// Determines whether or not the popup should be prevented to open.
        /// This property works OR-wise.
        /// </summary>
        public bool PreventOpening
        {
            get => preventOpening;
            set => preventOpening |= value;
        }
    }
}
namespace AcrylicKeyboard.Events
{
    public class PopupOpenEventArgs
    {
        private bool preventOpening = false;

        public bool PreventOpening
        {
            get => preventOpening;
            set => preventOpening |= value;
        }
    }
}
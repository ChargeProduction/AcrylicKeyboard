using System.Windows;

namespace AcrylicKeyboard.Events
{
    public class ResizeEventArgs
    {
        private Size panelSize;
        private Rect keyboardBounds;

        public ResizeEventArgs(Size panelSize, Rect keyboardBounds)
        {
            this.panelSize = panelSize;
            this.keyboardBounds = keyboardBounds;
        }

        public Size PanelSize => panelSize;

        public Rect KeyboardBounds => keyboardBounds;
    }
}
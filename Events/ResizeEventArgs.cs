using System.Windows;

namespace AcrylicKeyboard.Events
{
    public class ResizeEventArgs
    {
        private readonly Size canvasSize;
        private readonly Rect keyboardBounds;

        public ResizeEventArgs(Size canvasSize, Rect keyboardBounds)
        {
            this.canvasSize = canvasSize;
            this.keyboardBounds = keyboardBounds;
        }

        /// <summary>
        ///     Gets the new canvas size.
        /// </summary>
        public Size CanvasSize => canvasSize;

        /// <summary>
        ///     Gets the new keyboard bounds.
        /// </summary>
        public Rect KeyboardBounds => keyboardBounds;
    }
}
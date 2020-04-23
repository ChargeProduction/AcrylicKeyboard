using System;

namespace AcrylicKeyboard.Interaction
{
    public abstract class InputHandler : IDisposable
    {
        private Keyboard keyboard;
        private InteractionMode interactionMode;

        public abstract void Init();

        public InputHandler(Keyboard keyboard)
        {
            this.keyboard = keyboard;
        }

        /// <summary>
        /// Invalidates the current pointer position and should check if a key is hovered.
        /// Usually invoked after the layout has changed.
        /// </summary>
        public abstract void InvalidatePointerPosition();

        public abstract void Dispose();

        /// <summary>
        /// Gets the keyboard.
        /// </summary>
        public Keyboard Keyboard => keyboard;

        /// <summary>
        /// Determines which interaction mode is active.
        /// This value changes if the popup is opened.
        /// </summary>
        public InteractionMode InteractionMode
        {
            get => interactionMode;
            set => interactionMode = value;
        }
    }
}
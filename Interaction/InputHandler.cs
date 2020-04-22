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

        public abstract void InvalidatePointerPosition();

        public abstract void Dispose();
        
        public Keyboard Keyboard => keyboard;

        public InteractionMode InteractionMode
        {
            get => interactionMode;
            set => interactionMode = value;
        }
    }
}
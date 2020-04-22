using System.Windows;
using System.Windows.Input;
using AcrylicKeyboard.Layout;
using AcrylicKeyboard.Renderer;

namespace AcrylicKeyboard.Interaction
{
    public class MouseInteractionHandler : InputHandler
    {
        public const int KeyHoldingDelay = 500;
        private KeyInstance hoveringKey;
        private KeyInstance downKey;
        private Point pointerPosition;
        private bool hasInvokedHoldingAction = false;
        private UIElement capturedElement = null;

        public MouseInteractionHandler(Keyboard keyboard) : base(keyboard)
        {
        }

        public override void Init()
        {
            Keyboard.Canvas.MouseDown += OnMouseDown;
            Keyboard.Canvas.MouseUp += OnMouseUp;
            Keyboard.Canvas.MouseMove += OnMouseMove;
            Keyboard.Canvas.IsMouseDirectlyOverChanged += OnIsMouseDirectlyOverChanged;
        }

        public override void Dispose()
        {
            Keyboard.Canvas.MouseDown -= OnMouseDown;
            Keyboard.Canvas.MouseUp -= OnMouseUp;
            Keyboard.Canvas.MouseMove -= OnMouseMove;
            Keyboard.Canvas.IsMouseDirectlyOverChanged -= OnIsMouseDirectlyOverChanged;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            capturedElement?.ReleaseMouseCapture();
            switch (InteractionMode)
            {
                case InteractionMode.Popup:
                    downKey = hoveringKey;
                    hasInvokedHoldingAction = false;
                    break;
            }
            if (downKey != null)
            {
                SetMouseState(downKey, KeyMouseState.Hover);
                if (!hasInvokedHoldingAction)
                {
                    Keyboard.PerformAction(downKey, false);
                }

                downKey = null;
            }

            if (InteractionMode == InteractionMode.Popup)
            {
                Keyboard.PopupRenderer.HidePopup();
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            hasInvokedHoldingAction = false;
            downKey = hoveringKey;
            SetMouseState(downKey, KeyMouseState.Down);
            KeyHoldingAction.InvokeAsync(downKey, OnHoldingCallback, KeyHoldingDelay);
            
            capturedElement = (UIElement)sender;
            capturedElement.CaptureMouse();
        }

        private void OnHoldingCallback(KeyInstance obj)
        {
            if (obj != null && obj == downKey && obj.MouseState == KeyMouseState.Down)
            {
                SetMouseState(obj, KeyMouseState.Holding);
                Keyboard.PerformAction(obj, true);
                hasInvokedHoldingAction = true;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            pointerPosition = e.GetPosition(Keyboard.Canvas);
            InvalidatePointerPosition();
        }
        
        public override void InvalidatePointerPosition()
        {
            IKeyGroupRenderer renderer = null;
            switch (InteractionMode)
            {
                case InteractionMode.Keyboard:
                    renderer = Keyboard.KeyboardRenderer;
                    break;
                case InteractionMode.Popup:
                    renderer = Keyboard.PopupRenderer;
                    break;
            }
            var newHoveringKey = renderer?.GetKeyAt((int)pointerPosition.X, (int)pointerPosition.Y);
            
            if (downKey != newHoveringKey)
            {
                SetMouseState(downKey, KeyMouseState.Idle);
                downKey = null;
            }
            if (hoveringKey != newHoveringKey)
            {
                SetMouseState(hoveringKey, KeyMouseState.Idle);
                hoveringKey = newHoveringKey;
                SetMouseState(newHoveringKey, KeyMouseState.Hover);
            }
        }

        private void OnIsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetMouseState(hoveringKey, KeyMouseState.Idle);
            SetMouseState(downKey, KeyMouseState.Idle);
            hoveringKey = null;
            downKey = null;
        }

        private void SetMouseState(KeyInstance key, KeyMouseState value)
        {
            if (key != null)
            {;
                key.MouseState = value;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using AcrylicKeyboard.Interaction;
using AcrylicKeyboard.Layout;
using AcrylicKeyboard.Renderer.Animation;
using AcrylicKeyboard.Theme;

namespace AcrylicKeyboard.Renderer
{
    public class KeyboardPopupRenderer : IKeyGroupRenderer
    {
        private readonly Keyboard keyboard;
        private KeyInstance targetKey;
        private readonly SolidColorBrush background = new SolidColorBrush(Colors.Fuchsia);
        private readonly List<KeyInstance> popupKeys = new List<KeyInstance>();

        private readonly List<KeyValuePair<KeyInstance, KeyRenderer>> renderList =
            new List<KeyValuePair<KeyInstance, KeyRenderer>>();

        private readonly TransformAnimation animation = new TransformAnimation();

        private Rect bounds;

        public KeyboardPopupRenderer(Keyboard keyboard)
        {
            this.keyboard = keyboard;

            Init();
        }

        private void Init()
        {
            animation.AddFrames(animation.NewBuilder().WithDuration(0.1).WithScale(0));
            animation.AddFrames(animation.NewBuilder().WithScale(1));
        }

        /// <summary>
        ///     Shows the popup by setting the target key.
        /// </summary>
        /// <param name="key">The <see cref="KeyInstance" /> at which the popup should open.</param>
        public void ShowPopup(KeyInstance key)
        {
            Debug.Assert(key != null);
            targetKey = key;
            CreatePopupKeys();
            Keyboard.Animator.Play(animation);
        }

        /// <summary>
        ///     Hides the popup by setting the target key to null.
        /// </summary>
        public void HidePopup()
        {
            animation.End();
            targetKey = null;
            Keyboard.InputHandler.InteractionMode = InteractionMode.Keyboard;
        }

        public void OnUpdate(double delta)
        {
            if (IsVisible)
            {
                for (var i = 0; i < popupKeys.Count; i++)
                {
                    popupKeys[i].OnUpdate(delta);
                }
            }
        }

        public void OnRender(DrawingContext context)
        {
            if (IsVisible)
            {
                context.PushTransform(new TranslateTransform(Bounds.X, Bounds.Y));
                animation.PushTransform(context);

                RenderBase(context);
                for (var i = 0; i < renderList.Count; i++)
                {
                    renderList[i].Value.Render(Keyboard, context, renderList[i].Key, this);
                }

                animation.Pop(context);
                context.Pop();
            }
        }

        /// <summary>
        ///     Renders the popups background.
        /// </summary>
        private void RenderBase(DrawingContext context)
        {
            var backgroundColor = Keyboard.Theme.GetColor(ThemeColor.PopupBackground);
            if (background.Color != backgroundColor)
            {
                background.Color = backgroundColor;
            }

            context.DrawRectangle(background, null, new Rect(0, 0, Bounds.Width, Bounds.Height));
        }

        /// <summary>
        ///     Recreates the <see cref="KeyInstance" /> list which will be shown in the popup.
        /// </summary>
        private void CreatePopupKeys()
        {
            renderList.Clear();
            popupKeys.Clear();
            if (targetKey != null)
            {
                var extraKeys = targetKey.Settings.ExtraKeys;
                if (extraKeys != null)
                {
                    for (var i = 0; i < extraKeys.Length; i++)
                    {
                        var settings = extraKeys[i];
                        var key = Keyboard.Theme.GetKeyInstance(settings.Role);
                        var renderer = Keyboard.Theme.GetRenderer(settings.Role);
                        key.Init(Keyboard, settings);
                        renderList.Add(new KeyValuePair<KeyInstance, KeyRenderer>(key, renderer));
                        popupKeys.Add(key);
                    }

                    InsertTargetKey();
                }

                CalculateBounds();
            }

            Keyboard.InputHandler?.InvalidatePointerPosition();
        }

        /// <summary>
        ///     Inserts the target key at the fron or back, depending on the location of the popup.
        /// </summary>
        private void InsertTargetKey()
        {
            var targetClone = new KeyInstance();
            targetClone.Init(Keyboard, targetKey.Settings.Clone());
            targetClone.Settings.ExtraKeys = null;
            targetClone.Settings.ShowSecondaryKey = false;
            var targetRenderer = Keyboard.Theme.GetRenderer(targetKey.Settings.Role);
            var onLeftOfScreen = targetKey.Bounds.X < Keyboard.KeyboardBounds.Width / 2;
            if (onLeftOfScreen)
            {
                popupKeys.Insert(0, targetClone);
            }
            else
            {
                popupKeys.Insert(popupKeys.Count, targetClone);
            }

            renderList.Add(new KeyValuePair<KeyInstance, KeyRenderer>(targetClone, targetRenderer));
        }

        /// <summary>
        ///     Calculates bounds for every key in the popup.
        /// </summary>
        private void CalculateBounds()
        {
            if (targetKey != null)
            {
                var width = Keyboard.KeyboardRenderer.KeyWidth;
                var height = Keyboard.KeyboardRenderer.KeyHeight;
                for (var i = 0; i < popupKeys.Count; i++)
                {
                    popupKeys[i].Resize(new Rect(i * width, 0, width, height));
                }

                var onLeftOfScreen = targetKey.Bounds.X < Keyboard.KeyboardBounds.Width / 2;
                var offsetY = Math.Max(0, targetKey.Bounds.Y - height);
                var newBounds = new Rect(0, offsetY, width * popupKeys.Count, height);
                if (onLeftOfScreen)
                {
                    newBounds.X = Math.Max(0, targetKey.Bounds.X + targetKey.Bounds.Width - newBounds.Width);
                }
                else
                {
                    newBounds.X = targetKey.Bounds.X + width;
                    newBounds.X -= Math.Max(0, newBounds.Width);
                }

                newBounds.X += Keyboard.KeyboardBounds.X;
                newBounds.Y += Keyboard.KeyboardBounds.Y;

                bounds = newBounds;
                animation.AdjustFrame(0,
                    (ref TransformAnimation.Builder builder) =>
                        builder.WithPosition(bounds.Width / 2, bounds.Height / 2));
            }
        }

        /// <summary>
        ///     The target key at which the popup will be shown.
        /// </summary>
        public KeyInstance TargetKey => targetKey;

        public KeyInstance GetKeyAt(int x, int y)
        {
            var localX = x - (int) Bounds.X;
            var localY = y - (int) Bounds.Y;
            var keyboardX = x - (int) Keyboard.KeyboardBounds.X;
            var keyboardY = y - (int) Keyboard.KeyboardBounds.Y;
            if (TryGetDirectHit(localX, localY, out var result) ||
                TryHitTargetKey(keyboardX, keyboardY, out result) ||
                TryGetHorizontalHit(localX, out result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        ///     Tries to hit a key by checking point in rectangle collision.
        /// </summary>
        /// <param name="instance">The resulting <see cref="KeyInstance" /></param>
        /// <returns>Determines whether or not the hit test was successful.</returns>
        private bool TryGetDirectHit(int x, int y, out KeyInstance instance)
        {
            for (var i = 0; i < popupKeys.Count; i++)
            {
                if (popupKeys[i].Bounds.Contains(x, y))
                {
                    instance = popupKeys[i];
                    return true;
                }
            }

            instance = null;
            return false;
        }

        /// <summary>
        ///     Tries to hit the target key by checking point in rectangle collision.
        /// </summary>
        /// <param name="instance">The resulting <see cref="KeyInstance" /></param>
        /// <returns>Determines whether or not the hit test was successful.</returns>
        private bool TryHitTargetKey(int x, int y, out KeyInstance instance)
        {
            if (targetKey.Bounds.Contains(x, y))
            {
                instance = targetKey;
                return true;
            }

            instance = null;
            return false;
        }

        /// <summary>
        ///     Tries to hit a key by checking point in rectangle collision only along the x axis.
        /// </summary>
        /// <param name="instance">The resulting <see cref="KeyInstance" /></param>
        /// <returns>Determines whether or not the hit test was successful.</returns>
        private bool TryGetHorizontalHit(int x, out KeyInstance instance)
        {
            for (var i = 0; i < popupKeys.Count; i++)
            {
                if (x >= popupKeys[i].Bounds.X && x <= popupKeys[i].Bounds.X + popupKeys[i].Bounds.Width)
                {
                    instance = popupKeys[i];
                    return true;
                }
            }

            instance = null;
            return false;
        }

        /// <summary>
        ///     Gets the popup bounds.
        /// </summary>
        public Rect Bounds => bounds;

        /// <summary>
        ///     Gets the keyboard.
        /// </summary>
        public Keyboard Keyboard => keyboard;

        /// <summary>
        ///     Determines whether or not the popup is visible.
        /// </summary>
        public bool IsVisible => targetKey != null;
    }
}
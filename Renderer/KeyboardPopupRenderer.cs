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
        private Keyboard keyboard;
        private KeyInstance targetKey;
        private SolidColorBrush background = new SolidColorBrush(Colors.Fuchsia);
        private List<KeyInstance> popupKeys = new List<KeyInstance>();
        private List<KeyValuePair<KeyInstance, KeyRenderer>> renderList = new List<KeyValuePair<KeyInstance, KeyRenderer>>();
        
        private TransformAnimation animation = new TransformAnimation();

        private Rect bounds;

        public KeyboardPopupRenderer(Keyboard keyboard)
        {
            this.keyboard = keyboard;

            Init();
        }

        private void Init()
        {
            animation.AddFrame(new TransformAnimation.Builder().WithDuration(100).WithScale(0));
            animation.AddFrame(new TransformAnimation.Builder().WithScale(1));
        }

        public void ShowPopup(KeyInstance key)
        {
            Debug.Assert(key != null);
            this.targetKey = key;
            CreatePopupKeys();
            animation.Start();
        }

        public void HidePopup()
        {
            animation.End();
            this.targetKey = null;
            Keyboard.InputHandler.InteractionMode = InteractionMode.Keyboard;
        }

        public void OnRender(DrawingContext c)
        {
            if (IsVisible)
            {
                animation.Update();
                for (var i = 0; i < popupKeys.Count; i++)
                {
                    popupKeys[i].OnUpdate();
                }
                c.PushTransform(new TranslateTransform(Bounds.X, Bounds.Y));
                animation.PushTransform(c);
                
                RenderBase(c);
                for (var i = 0; i < renderList.Count; i++)
                {
                    renderList[i].Value.Render(Keyboard, c, renderList[i].Key, this);
                }
                
                animation.Pop(c);
                c.Pop();
            }
        }

        private void RenderBase(DrawingContext c)
        {
            var backgroundColor = Keyboard.Theme.GetColor(ThemeColor.PopupBackground);
            if (background.Color != backgroundColor)
            {
                background.Color = backgroundColor;
            }
            c.DrawRectangle(background, null, new Rect(0, 0, Bounds.Width, Bounds.Height));
        }

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

        private void InsertTargetKey()
        {
            var targetClone = new KeyInstance();
            targetClone.Init(Keyboard, targetKey.Settings.Clone());
            targetClone.Settings.ExtraKeys = null;
            targetClone.Settings.ShowSecondaryKey = false;
            var targetRenderer = Keyboard.Theme.GetRenderer(targetKey.Settings.Role);
            bool onLeftOfScreen = targetKey.Bounds.X < Keyboard.KeyboardBounds.Width / 2;
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

        private void CalculateBounds()
        {
            if (targetKey != null)
            {
                var width = Keyboard.KeyboardRenderer.KeyWidth;
                var height = Keyboard.KeyboardRenderer.KeyHeight;
                for (var i = 0; i < popupKeys.Count; i++)
                {
                    popupKeys[i].Resize(new Rect(i * width, 0, width, height), this);
                }
                
                bool onLeftOfScreen = targetKey.Bounds.X < Keyboard.KeyboardBounds.Width / 2;
                var offsetY = Math.Max(0, targetKey.Bounds.Y - height);
                Rect newBounds = new Rect(0, offsetY, width * popupKeys.Count, height);
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

                this.bounds = newBounds;
                animation.AdjustFrame(0, (frame) => frame.WithPosition(bounds.Width / 2, bounds.Height / 2));
            }
        }

        public KeyInstance TargetKey => targetKey;

        public KeyInstance GetKeyAt(int x, int y)
        {
            var localX = x - (int)Bounds.X;
            var localY = y - (int)Bounds.Y;
            var keyboardX = x - (int)Keyboard.KeyboardBounds.X;
            var keyboardY = y - (int)Keyboard.KeyboardBounds.Y;
            if (TryGetDirectHit(localX, localY, out var result) || 
                TryHitBaseKey(keyboardX, keyboardY, out result) ||
                TryGetHorizontalHit(localX, out result))
            {
                return result;
            }

            return null;
        }

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

        private bool TryHitBaseKey(int x, int y, out KeyInstance instance)
        {
            if (targetKey.Bounds.Contains(x, y))
            {
                instance = targetKey;
                return true;
            }
            instance = null;
            return false;
        }

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

        public int KeyGap
        {
            get => Keyboard.KeyboardRenderer?.KeyGap ?? 4;
        }

        public Rect Bounds => bounds;

        public Keyboard Keyboard
        {
            get => keyboard;
        }
        
        public bool IsVisible
        {
            get => targetKey != null;
        }
    }
}
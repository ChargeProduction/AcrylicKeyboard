using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using AcrylicKeyboard.Interaction;
using AcrylicKeyboard.Renderer;
using AcrylicKeyboard.Theme;

namespace AcrylicKeyboard.Layout
{
    public class KeyInstance
    {
        private KeyMouseState mouseState = KeyMouseState.Idle;
        private Keyboard keyboard;
        private KeySettings settings;
        
        private Rect bounds;
        private Rect primaryBounds;
        private Rect secondaryBounds;

        private SolidColorBrush backgroundBrush = new SolidColorBrush(Colors.Fuchsia);
        private SolidColorBrush foregroundBrush = new SolidColorBrush(Colors.Fuchsia);

        private KeyGeometryBuilder primaryKey;
        private KeyGeometryBuilder secondaryKey;

        internal void Init (Keyboard keyboard, KeySettings settings)
        {
            Debug.Assert(keyboard != null);
            Debug.Assert(settings != null);

            this.keyboard = keyboard;
            this.settings = settings;

            LoadFonts();
        }

        private void LoadFonts()
        {
            var config = Keyboard.GetLayoutConfig();
            String selectedFont;
            if (settings.IsIcon)
            {
                selectedFont = settings.CustomFont ?? config.IconFont ?? "Segoe MDL2 Assets";
            }
            else
            {
                selectedFont = settings.CustomFont ?? config.Font ?? "Segoe UI Light";
            }
            secondaryKey = new KeyGeometryBuilder(new FontFamily(selectedFont));
            primaryKey = new KeyGeometryBuilder(new FontFamily(selectedFont));
            OnInit();
        }

        protected virtual void OnInit()
        {
            
        }

        public virtual void OnUpdate()
        {
            
            primaryKey.Text = Settings.IsIcon ? Settings.Icon : keyboard.GetKeyText(Settings, Settings.DisplayText);
            if (Settings.ShowSecondaryKey && Settings.ExtraKeys != null && Settings.ExtraKeys.Length > 0)
            {
                var extraKeySettings = Settings.ExtraKeys[0];
                secondaryKey.Text = extraKeySettings.IsIcon ? extraKeySettings.Icon : keyboard.GetKeyText(extraKeySettings, extraKeySettings?.DisplayText);
            }

            switch (MouseState)
            {
                case KeyMouseState.Idle:
                    var bg = ThemeColor.KeyBackground;
                    var fg = ThemeColor.KeyForeground;
                    if (Settings.KnownRole != KeyRole.Default)
                    {
                        bg = ThemeColor.KeyWithRoleBackground;
                        fg = ThemeColor.KeyWithRoleForeground;
                    }
                    backgroundBrush.Color = keyboard.Theme.GetColor(bg);
                    foregroundBrush.Color = keyboard.Theme.GetColor(fg);
                    break;
                case KeyMouseState.Hover:
                    backgroundBrush.Color = keyboard.Theme.GetColor(ThemeColor.KeyHoverBackground);
                    foregroundBrush.Color = keyboard.Theme.GetColor(ThemeColor.KeyHoverForeground);
                    break;
                case KeyMouseState.Down:
                    backgroundBrush.Color = keyboard.Theme.GetColor(ThemeColor.KeyDownBackground);
                    foregroundBrush.Color = keyboard.Theme.GetColor(ThemeColor.KeyDownForeground);
                    break;
            }
        }

        public void Resize(Rect bounds, IKeyGroupRenderer renderer)
        {
            var previousBounds = this.Bounds;
            this.bounds = bounds;
            RecalculateBounds();
            OnResize(previousBounds);
        }

        protected virtual void OnResize(Rect previousBounds)
        {
            
        }

        private void RecalculateBounds()
        {
            double middleX = Bounds.Width / 2d;
            double middleY = Bounds.Height / 2d;
            var gap = Keyboard.KeyGap;
            double widthHalf = Math.Max(1, middleX - gap * 2) / 2;
            double heightHalf = Math.Max(1, middleY - gap * 2) / 2;

            double heightModifier = 2.35;
            if (settings.IsIcon)
            {
                middleY *= 1.08;
            }
            primaryBounds = new Rect(Math.Max(0, middleX - widthHalf), Math.Max(0, middleY - heightHalf), widthHalf * 2, heightHalf * heightModifier);
            secondaryBounds = new Rect(0, 0, Math.Max(0, Bounds.Width - gap * 2 - 10), Math.Max(0, Bounds.Height / 4));
            secondaryBounds.X = gap + 5;
            secondaryBounds.Y = gap + 2 + secondaryBounds.Height / 4;
        }

        public void ApplyStates(KeyInstance instance)
        {
            this.mouseState = instance.mouseState;
            this.primaryBounds = instance.primaryBounds;
            this.secondaryBounds = instance.secondaryBounds;
            this.backgroundBrush = instance.backgroundBrush;
            this.foregroundBrush = instance.foregroundBrush;
            this.primaryKey = instance.primaryKey;
            this.secondaryKey = instance.secondaryKey;
            OnApplyState(instance);
        }

        protected virtual void OnApplyState(KeyInstance instance)
        {
            
        }

        protected virtual KeyInstance OnClone()
        {
            return new KeyInstance();
        }

        public Keyboard Keyboard => keyboard;

        public KeySettings Settings => settings;

        public Rect Bounds => bounds;

        public Rect PrimaryBounds => primaryBounds;

        public Rect SecondaryBounds => secondaryBounds;

        public KeyGeometryBuilder PrimaryKey => primaryKey;

        public KeyGeometryBuilder SecondaryKey => secondaryKey;

        public KeyMouseState MouseState
        {
            get => mouseState;
            set => mouseState = value;
        }

        public SolidColorBrush BackgroundBrush
        {
            get => backgroundBrush;
            set => backgroundBrush = value;
        }

        public SolidColorBrush ForegroundBrush
        {
            get => foregroundBrush;
            set => foregroundBrush = value;
        }
    }
}
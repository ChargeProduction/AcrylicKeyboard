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

        internal void Init(Keyboard keyboard, KeySettings settings)
        {
            Debug.Assert(keyboard != null);
            Debug.Assert(settings != null);

            this.keyboard = keyboard;
            this.settings = settings;

            LoadFonts();
            OnInit();
        }

        /// <summary>
        ///     Loads the font for this key depending on if the icon property of <see cref="Settings" /> was set.
        /// </summary>
        private void LoadFonts()
        {
            var config = Keyboard.GetLayoutConfig();
            string selectedFont;
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
        }

        protected virtual void OnInit()
        {
        }

        /// <summary>
        ///     Updates the keys text and background colors.
        /// </summary>
        public virtual void OnUpdate(double delta)
        {
            primaryKey.Text = Settings.IsIcon ? Settings.Icon : keyboard.GetKeyText(Settings, Settings.DisplayText);
            if (Settings.ShowSecondaryKey && Settings.ExtraKeys != null && Settings.ExtraKeys.Length > 0)
            {
                var extraKeySettings = Settings.ExtraKeys[0];
                secondaryKey.Text = extraKeySettings.IsIcon
                    ? extraKeySettings.Icon
                    : keyboard.GetKeyText(extraKeySettings, extraKeySettings?.DisplayText);
            }

            switch (MouseState)
            {
                case KeyMouseState.Idle:
                    var bg = ThemeColor.KeyBackground;
                    var fg = ThemeColor.KeyForeground;
                    if (Settings.KnownModifier != KeyModifier.None)
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

        public void Resize(Rect bounds)
        {
            var previousBounds = Bounds;
            this.bounds = bounds;
            RecalculateBounds();
            OnResize(previousBounds);
        }

        protected virtual void OnResize(Rect previousBounds)
        {
        }

        /// <summary>
        ///     Recalculates the key bounds.
        ///     Primary bounds define the display text bounds.
        ///     Secondary bounds define the upper left text of the first extra key if set.
        /// </summary>
        private void RecalculateBounds()
        {
            var middleX = Bounds.Width / 2d;
            var middleY = Bounds.Height / 2d;
            var gap = Keyboard.KeyGap;
            var widthHalf = Math.Max(1, middleX - gap * 2) / 2;
            var heightHalf = Math.Max(1, middleY - gap * 2) / 2;

            var heightModifier = 2.35;
            if (settings.IsIcon)
            {
                middleY *= 1.08;
            }

            primaryBounds = new Rect(Math.Max(0, middleX - widthHalf), Math.Max(0, middleY - heightHalf), widthHalf * 2,
                heightHalf * heightModifier);
            secondaryBounds = new Rect(0, 0, Math.Max(0, Bounds.Width - gap * 2 - 10), Math.Max(0, Bounds.Height / 4));
            secondaryBounds.X = gap + 5;
            secondaryBounds.Y = gap + 2 + secondaryBounds.Height / 4;
        }

        /// <summary>
        ///     Applies the current state to a new key instance.
        ///     This helps to keep hovering states but also reduces time consuming font recalculation.
        /// </summary>
        /// <param name="instance"></param>
        public void ApplyStates(KeyInstance instance)
        {
            mouseState = instance.mouseState;
            primaryBounds = instance.primaryBounds;
            secondaryBounds = instance.secondaryBounds;
            backgroundBrush = instance.backgroundBrush;
            foregroundBrush = instance.foregroundBrush;
            primaryKey = instance.primaryKey;
            secondaryKey = instance.secondaryKey;
            OnApplyState(instance);
        }

        protected virtual void OnApplyState(KeyInstance instance)
        {
        }

        /// <summary>
        ///     Gets the keyboard.
        /// </summary>
        public Keyboard Keyboard => keyboard;

        /// <summary>
        ///     Gets the keys settings.
        /// </summary>
        public KeySettings Settings => settings;

        /// <summary>
        ///     Gets the keys total bounds.
        /// </summary>
        public Rect Bounds => bounds;

        /// <summary>
        ///     Gets the bounds of the display text.
        /// </summary>
        public Rect PrimaryBounds => primaryBounds;

        /// <summary>
        ///     Gets the bounds used to display the first extra key.
        /// </summary>
        public Rect SecondaryBounds => secondaryBounds;

        /// <summary>
        ///     Gets the primary text geometry builder.
        /// </summary>
        public KeyGeometryBuilder PrimaryKey => primaryKey;

        /// <summary>
        ///     Gets the secondary text geometry builder.
        /// </summary>
        public KeyGeometryBuilder SecondaryKey => secondaryKey;

        /// <summary>
        ///     Gets or sets the mouse state.
        /// </summary>
        public KeyMouseState MouseState
        {
            get => mouseState;
            set => mouseState = value;
        }

        /// <summary>
        ///     Gets or sets the background brush.
        /// </summary>
        public SolidColorBrush BackgroundBrush
        {
            get => backgroundBrush;
            set => backgroundBrush = value;
        }

        /// <summary>
        ///     Gets or sets the foreground brush.
        /// </summary>
        public SolidColorBrush ForegroundBrush
        {
            get => foregroundBrush;
            set => foregroundBrush = value;
        }
    }
}
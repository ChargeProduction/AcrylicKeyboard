using AcrylicKeyboard.Theme;

namespace AcrylicKeyboard.Events
{
    public class ThemeChangedEventArgs
    {
        private readonly KeyboardTheme theme;

        public ThemeChangedEventArgs(KeyboardTheme theme)
        {
            this.theme = theme;
        }

        /// <summary>
        ///     Gets the new keyboard theme.
        /// </summary>
        public KeyboardTheme Theme => theme;
    }
}
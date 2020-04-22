using AcrylicKeyboard.Theme;

namespace AcrylicKeyboard.Events
{
    public class ThemeChangedEventArgs
    {
        private KeyboardTheme theme;

        public ThemeChangedEventArgs(KeyboardTheme theme)
        {
            this.theme = theme;
        }

        public KeyboardTheme Theme => theme;
    }
}
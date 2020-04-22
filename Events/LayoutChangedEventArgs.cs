using System;
using AcrylicKeyboard.Layout;

namespace AcrylicKeyboard.Events
{
    public class LayoutChangedEventArgs
    {
        private String language;
        private String layout;
        private KeyboardLayoutConfig config;

        public LayoutChangedEventArgs(string language, string layout, KeyboardLayoutConfig config)
        {
            this.language = language;
            this.layout = layout;
            this.config = config;
        }

        public string Language => language;

        public string Layout => layout;

        public KeyboardLayoutConfig Config => config;
    }
}
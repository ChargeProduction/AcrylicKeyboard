using AcrylicKeyboard.Layout;

namespace AcrylicKeyboard.Events
{
    public class LayoutChangedEventArgs
    {
        private readonly string language;
        private readonly string layout;
        private readonly KeyboardLayoutConfig config;

        public LayoutChangedEventArgs(string language, string layout, KeyboardLayoutConfig config)
        {
            this.language = language;
            this.layout = layout;
            this.config = config;
        }

        /// <summary>
        ///     Gets the new language string.
        /// </summary>
        public string Language => language;

        /// <summary>
        ///     Gets the new layout string.
        /// </summary>
        public string Layout => layout;

        /// <summary>
        ///     Gets the new active keyboard configuration.
        /// </summary>
        public KeyboardLayoutConfig Config => config;
    }
}
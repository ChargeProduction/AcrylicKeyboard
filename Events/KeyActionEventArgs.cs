using System.Collections.Generic;
using AcrylicKeyboard.Interaction;

namespace AcrylicKeyboard.Events
{
    public class KeyActionEventArgs
    {
        private readonly KeyboardAction action;
        private readonly string text;
        private readonly List<KeyModifier> activeModifiers;

        public KeyActionEventArgs(KeyboardAction action, string text = null, List<KeyModifier> activeModifiers = null)
        {
            this.action = action;
            this.text = text;
            this.activeModifiers = activeModifiers ?? new List<KeyModifier>();
        }

        /// <summary>
        ///     Gets the key action.
        /// </summary>
        public KeyboardAction Action => action;

        /// <summary>
        ///     Gets the inserted text.
        /// </summary>
        public string Text => text;

        /// <summary>
        ///     Gets a list of active modifiers.
        /// </summary>
        public IReadOnlyList<KeyModifier> ActiveModifiers => activeModifiers;
    }
}
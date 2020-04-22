using System;
using System.Collections.Generic;
using AcrylicKeyboard.Interaction;

namespace AcrylicKeyboard.Events
{
    public class KeyActionEventArgs
    {
        private KeyboardAction action;
        private String text;
        private List<KeyRole> activeModifiers;

        public KeyActionEventArgs(KeyboardAction action, string text = null, List<KeyRole> activeModifiers = null)
        {
            this.action = action;
            this.text = text;
            this.activeModifiers = activeModifiers ?? new List<KeyRole>();
        }

        public KeyboardAction Action => action;

        public string Text => text;

        public IReadOnlyList<KeyRole> ActiveModifiers => activeModifiers;
    }
}
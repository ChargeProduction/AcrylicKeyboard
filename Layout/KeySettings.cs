using System;
using System.Globalization;
using AcrylicKeyboard.Interaction;
using AcrylicKeyboard.Properties;
using Newtonsoft.Json;

namespace AcrylicKeyboard.Layout
{
    public class KeySettings
    {
        private string identity;
        private string icon;
        private string displayText;
        private string insertionText;
        private string customFont;
        private KeySettings[] extraKeys;
        private KeyboardAction action = KeyboardAction.InsertOnText;
        private KeyboardAction holdingAction = KeyboardAction.Nothing;
        private bool showSecondaryKey;
        [NotNull] private string size = "1";
        private string target;
        private bool ignoreCap;
        private bool isVisible = true;
        [NotNull] private string role = "";

        // Automatically computed fields.
        private KeyModifier knownModifier = KeyModifier.None;
        private double sizeValue = 1;
        private bool isSizeStar;

        /// <summary>
        ///     Gets or sets the keys identity.
        ///     Keys with same identity share the same state after the layout has changed.
        ///     Go to <see cref="KeyInstance.ApplyStates" /> to see which states are applied after layout has changed.
        /// </summary>
        [JsonProperty("id")]
        public string Identity
        {
            get => identity;
            set => identity = value;
        }

        /// <summary>
        ///     Gets or sets the keys icon.
        ///     If the icon is set, no display text will be shown.
        /// </summary>
        [JsonProperty("icon")]
        public string Icon
        {
            get => icon;
            set => icon = value;
        }

        /// <summary>
        ///     Gets or sets the keys display text.
        ///     If no insertion text is set, the insertion text is set to its value.
        /// </summary>
        [JsonProperty("displayText")]
        public string DisplayText
        {
            get => displayText;
            set
            {
                displayText = value;
                if (InsertionText == null)
                {
                    InsertionText = DisplayText;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the insertion text.
        ///     The insertion text will be inserted on keypress if the <see cref="Action" /> or <see cref="HoldingAction" />
        ///     is set to <see cref="KeyboardAction.InsertOnText" /> (which is the default).
        /// </summary>
        [JsonProperty("insertionText")]
        public string InsertionText
        {
            get => insertionText;
            set => insertionText = value;
        }

        /// <summary>
        ///     Gets or sets the font for this specific key.-
        /// </summary>
        [JsonProperty("customFont")]
        public string CustomFont
        {
            get => customFont;
            set => customFont = value;
        }

        /// <summary>
        ///     Gets or sets an array of extra key which will be shown as popup on holding.
        /// </summary>
        [JsonProperty("extraKeys")]
        public KeySettings[] ExtraKeys
        {
            get => extraKeys;
            set => extraKeys = value;
        }

        /// <summary>
        ///     Gets or sets the keys action.
        ///     An action determines what happens on release.
        ///     Default value: <see cref="KeyboardAction.InsertOnText" />
        /// </summary>
        [JsonProperty("action")]
        public KeyboardAction Action
        {
            get => action;
            set => action = value;
        }

        /// <summary>
        ///     Gets or sets the keys action on holding.
        ///     A holding action determines what happens after the key was held down for a specific amount of time.
        ///     Default value: <see cref="KeyboardAction.InsertOnText" />
        /// </summary>
        [JsonProperty("holdingAction")]
        public KeyboardAction HoldingAction
        {
            get => holdingAction;
            set => holdingAction = value;
        }

        /// <summary>
        ///     Determines whether or not the <see cref="DisplayText" /> of the first <see cref="ExtraKeys" /> item
        ///     should be shown at the top left.
        /// </summary>
        [JsonProperty("showSecondaryText")]
        public bool ShowSecondaryKey
        {
            get => showSecondaryKey;
            set => showSecondaryKey = value;
        }

        /// <summary>
        ///     Gets or sets the key size. The key size will be multiplied by the calculated width of a key.
        ///     To fill the remain space the "*" (star) character can be used.
        ///     If null is assigned, size becomes "*" (star).
        ///     Default value: 1
        /// </summary>
        [JsonProperty("size")]
        [NotNull]
        public string Size
        {
            get => size;
            set
            {
                size = value ?? "*";
                isSizeStar = true;
                if (!string.IsNullOrEmpty(size))
                {
                    if (double.TryParse(size, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                        out sizeValue))
                    {
                        isSizeStar = false;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the target property. The target is a parameter for <see cref="Action" /> or
        ///     <see cref="HoldingAction" />.
        ///     For example if <see cref="Action" /> is set to <see cref="KeyboardAction.ChangeLanguage" />, this property
        ///     would define the language to switch to.
        /// </summary>
        [JsonProperty("target")]
        public string Target
        {
            get => target;
            set => target = value;
        }

        /// <summary>
        ///     Determines whether or not the <see cref="DisplayText" /> should change case on shift or caps lock.
        /// </summary>
        [JsonProperty("ignoreCap")]
        public bool IgnoreCap
        {
            get => ignoreCap;
            set => ignoreCap = value;
        }

        /// <summary>
        ///     Determines whether or not the key is visible. If the key is not visible, no hit testing or rendering occurs
        ///     but space is added.
        /// </summary>
        [JsonProperty("isVisible")]
        public bool IsVisible
        {
            get => isVisible;
            set => isVisible = value;
        }

        /// <summary>
        ///     Gets or sets the keys role. A role is used for modification keys like <see cref="KeyModifier.Shift" /> or
        ///     <see cref="KeyModifier.Ctrl" /> but can also be used to identify similar keys and for example paint them in
        ///     the same color when one is activated.
        ///     The role can be set to any value but if matching (ignoring case) the predefined roles from
        ///     <see cref="KeyModifier" /> (except
        ///     <see cref="KeyModifier.None">
        ///         , the key will have special behaviour.
        ///         Default value: <see cref="KeyModifier.None">.
        /// </summary>
        [JsonProperty("role")]
        [NotNull]
        public string Role
        {
            get => role;
            set
            {
                role = value ?? "";
                if (!Enum.TryParse(role, true, out knownModifier))
                {
                    knownModifier = KeyModifier.None;
                }
            }
        }

        /// <summary>
        ///     Gets the parsed size value of the key.
        /// </summary>
        [JsonIgnore]
        public double SizeValue => sizeValue;

        /// <summary>
        ///     Determines whether or not the key should fill remaining space.
        /// </summary>
        [JsonIgnore]
        public bool IsSizeStar => isSizeStar;

        /// <summary>
        ///     Gets the role as <see cref="KeyModifier" /> if matches (ignoring case).
        /// </summary>
        [JsonIgnore]
        public KeyModifier KnownModifier => knownModifier;

        /// <summary>
        ///     Determines whether or not the key has an icon.
        /// </summary>
        [JsonIgnore]
        public bool IsIcon => !string.IsNullOrEmpty(icon);

        /// <summary>
        ///     Clones the <see cref="KeySettings" />.
        /// </summary>
        /// <returns>A new instance of <see cref="KeySettings" /> with copied field values.</returns>
        public KeySettings Clone()
        {
            var copy = new KeySettings();
            copy.Identity = identity;
            copy.Icon = icon;
            copy.CustomFont = CustomFont;
            copy.DisplayText = DisplayText;
            copy.InsertionText = InsertionText;
            if (ExtraKeys != null)
            {
                copy.ExtraKeys = new KeySettings[ExtraKeys.Length];
                for (var i = 0; i < ExtraKeys.Length; i++)
                {
                    copy.ExtraKeys[i] = ExtraKeys[i].Clone();
                }
            }

            copy.Action = Action;
            copy.HoldingAction = HoldingAction;
            copy.ShowSecondaryKey = ShowSecondaryKey;
            copy.Size = Size;
            copy.IgnoreCap = IgnoreCap;
            copy.IsVisible = IsVisible;
            copy.Role = Role;
            return copy;
        }
    }
}
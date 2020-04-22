using System;
using System.Globalization;
using AcrylicKeyboard.Interaction;
using AcrylicKeyboard.Properties;
using Newtonsoft.Json;

namespace AcrylicKeyboard.Layout
{
    public class KeySettings
    {
        private String identity;
        private String icon;
        private String displayText;
        private String insertionText;
        private String customFont;
        private KeySettings[] extraKeys;
        private KeyboardAction action = KeyboardAction.InsertOnText;
        private KeyboardAction holdingAction = KeyboardAction.Nothing;
        private bool showSecondaryKey;
        [NotNull] private String size = "1";
        private String target;
        private bool ignoreCap;
        private bool isVisible = true;
        [NotNull] private String role = "";
        
        private KeyRole knownRole = KeyRole.Default;
        private double sizeValue = 1;
        private bool isSizeStar = false;

        [JsonProperty("id")]
        public string Identity
        {
            get => identity;
            set => identity = value;
        }

        [JsonProperty("icon")]
        public string Icon
        {
            get => icon;
            set => icon = value;
        }

        [JsonProperty("displayText")]
        public String DisplayText
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

        [JsonProperty("insertionText")]
        public String InsertionText
        {
            get => insertionText;
            set => insertionText = value;
        }

        [JsonProperty("customFont")]
        public string CustomFont
        {
            get => customFont;
            set => customFont = value;
        }

        [JsonProperty("extraKeys")]
        public KeySettings[] ExtraKeys
        {
            get => extraKeys;
            set => extraKeys = value;
        }

        [JsonProperty("action")]
        public KeyboardAction Action
        {
            get => action;
            set => action = value;
        }

        [JsonProperty("holdingAction")]
        public KeyboardAction HoldingAction
        {
            get => holdingAction;
            set => holdingAction = value;
        }

        [JsonProperty("showSecondaryText")]
        public bool ShowSecondaryKey
        {
            get => showSecondaryKey;
            set => showSecondaryKey = value;
        }

        [JsonProperty("size")]
        [NotNull]
        public String Size
        {
            get => size;
            set
            {
                size = value ?? "*";
                isSizeStar = true;
                if (!String.IsNullOrEmpty(size))
                {
                    if (Double.TryParse(size, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out sizeValue))
                    {
                        isSizeStar = false;
                    }
                }
            }
        }

        [JsonProperty("target")]
        public string Target
        {
            get => target;
            set => target = value;
        }

        [JsonProperty("ignoreCap")]
        public bool IgnoreCap
        {
            get => ignoreCap;
            set => ignoreCap = value;
        }

        [JsonProperty("isVisible")]
        public bool IsVisible
        {
            get => isVisible;
            set => isVisible = value;
        }

        [JsonProperty("role")]
        [NotNull]
        public String Role
        {
            get => role;
            set
            {
                role = value ?? "";
                if (!Enum.TryParse(role, true, out knownRole))
                {
                    knownRole = KeyRole.Default;
                }
            }
        }

        [JsonIgnore]
        public double SizeValue => sizeValue;

        [JsonIgnore]
        public bool IsSizeStar => isSizeStar;

        [JsonIgnore]
        public KeyRole KnownRole => knownRole;

        [JsonIgnore]
        public bool IsIcon => !String.IsNullOrEmpty(icon);

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
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace AcrylicKeyboard.Layout
{
    public class KeyboardLayoutConfig
    {
        private string title;
        private string font;
        private string iconFont;
        private Dictionary<string, KeySettings[][]> layouts = new Dictionary<string, KeySettings[][]>();

        public static KeyboardLayoutConfig FromFile(string path)
        {
            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<KeyboardLayoutConfig>(File.ReadAllText(path));
            }

            return null;
        }

        /// <summary>
        ///     Gets or sets the layout title.
        /// </summary>
        [JsonProperty("title")]
        public string Title
        {
            get => title;
            set => title = value;
        }

        /// <summary>
        ///     Gets or sets the keys display text font.
        /// </summary>
        [JsonProperty("font")]
        public string Font
        {
            get => font;
            set => font = value;
        }

        /// <summary>
        ///     Gets or sets the keys icon font.
        /// </summary>
        [JsonProperty("iconFont")]
        public string IconFont
        {
            get => iconFont;
            set => iconFont = value;
        }

        /// <summary>
        ///     Gets or sets the key matrix for this layout.
        /// </summary>
        [JsonProperty("layouts")]
        public Dictionary<string, KeySettings[][]> Layouts
        {
            get => layouts;
            set => layouts = value;
        }
    }
}
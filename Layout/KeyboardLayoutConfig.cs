using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace AcrylicKeyboard.Layout
{
    public class KeyboardLayoutConfig
    {
        private String title;
        private String font;
        private String iconFont;
        private Dictionary<String, KeySettings[][]> layouts = new Dictionary<string, KeySettings[][]>();

        public static KeyboardLayoutConfig FromFile(String path)
        {
            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<KeyboardLayoutConfig>(File.ReadAllText(path));
            }

            return null;
        }

        [JsonProperty("title")]
        public string Title
        {
            get => title;
            set => title = value;
        }

        [JsonProperty("font")]
        public string Font
        {
            get => font;
            set => font = value;
        }

        [JsonProperty("iconFont")]
        public string IconFont
        {
            get => iconFont;
            set => iconFont = value;
        }

        [JsonProperty("layouts")]
        public Dictionary<string, KeySettings[][]> Layouts
        {
            get => layouts;
            set => layouts = value;
        }
    }
}
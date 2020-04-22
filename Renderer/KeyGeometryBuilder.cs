using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace AcrylicKeyboard.Renderer
{
    public class KeyGeometryBuilder
    {
        private FontFamily font;
        private String text;
        private FormattedText formattedText;
        private bool isDirty = true;

        public KeyGeometryBuilder(FontFamily font)
        {
            this.font = font;
        }

        private void BuildText()
        {
            if (!String.IsNullOrEmpty(text?.Trim()))
            {
                formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    new Typeface(font, FontStyles.Normal, FontWeights.Thin, FontStretches.Normal), 15,
                    new SolidColorBrush(Colors.White));
                isDirty = false;
            }
        }

        public FormattedText FormattedText
        {
            get
            {
                if (isDirty)
                {
                    BuildText();
                }
                return formattedText;
            }
        }

        public string Text
        {
            get => text;
            set
            {
                if (!String.Equals(text, value))
                {
                    text = value;
                    isDirty = true;
                }
            }
        }
    }
}
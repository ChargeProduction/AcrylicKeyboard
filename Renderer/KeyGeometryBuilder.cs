using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace AcrylicKeyboard.Renderer
{
    public class KeyGeometryBuilder
    {
        private readonly FontFamily font;
        private string text;
        private FormattedText formattedText;
        private bool isDirty = true;

        public KeyGeometryBuilder(FontFamily font)
        {
            this.font = font;
        }

        /// <summary>
        ///     Creates the new formatted text object.
        /// </summary>
        private void BuildText()
        {
            if (!string.IsNullOrEmpty(text?.Trim()))
            {
                formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    new Typeface(font, FontStyles.Normal, FontWeights.Thin, FontStretches.Normal), 15,
                    new SolidColorBrush(Colors.White));
                isDirty = false;
            }
        }

        /// <summary>
        ///     Gets the formatted text object.
        /// </summary>
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

        /// <summary>
        ///     Gets or sets the text.
        /// </summary>
        public string Text
        {
            get => text;
            set
            {
                if (!string.Equals(text, value))
                {
                    text = value;
                    isDirty = true;
                }
            }
        }
    }
}
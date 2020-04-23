using System;
using System.Windows;
using System.Windows.Media;
using AcrylicKeyboard.Interaction;
using AcrylicKeyboard.Layout;
using AcrylicKeyboard.Theme;

namespace AcrylicKeyboard.Renderer
{
    public class KeyRenderer
    {
        // private DrawingGroup backingStore = new DrawingGroup();
        
        private static SolidColorBrush focusBackgroundBrush = new SolidColorBrush(Colors.Fuchsia);
        private static SolidColorBrush focusForegroundBrush = new SolidColorBrush(Colors.Fuchsia);

        /// <summary>
        /// Initializes the renderer once on startup.
        /// </summary>
        /// <param name="systemContext"></param>
        internal void Init(DrawingContext systemContext)
        {
            //TODO optimize rendering speed by using: systemContext.DrawDrawing(backingStore);
        }
        
        public void Update(Keyboard keyboard, double delta)
        {
            focusBackgroundBrush.Color = keyboard.Theme.GetColor(ThemeColor.KeyDownBackground);
            focusForegroundBrush.Color = keyboard.Theme.GetColor(ThemeColor.KeyDownForeground);
        }
        
        public void Render(Keyboard keyboard, DrawingContext context, KeyInstance instance, IKeyGroupRenderer renderer)
        {
            if (instance.Settings.IsVisible)
            {
                var gap = keyboard.KeyGap;
                double actualWidth = instance.Bounds.Width - gap * 2;
                double actualHeight = instance.Bounds.Height - gap * 2;
                if (actualWidth > 0 && actualHeight > 0)
                {
                    context.PushTransform(new TranslateTransform(instance.Bounds.X, instance.Bounds.Y));

                    var backgroundColor = instance.BackgroundBrush;
                    var foregroundColor = instance.ForegroundBrush;

                    var role = instance.Settings.KnownModifier;
                    if ((role != KeyModifier.Shift && keyboard.IsModifierActive(role)) || 
                        (role == KeyModifier.Shift && WinApiHelper.IsCapsLock))
                    {
                        backgroundColor = focusBackgroundBrush;
                        foregroundColor = focusForegroundBrush;
                    }
                    
                    context.DrawRectangle(backgroundColor, null, new Rect(gap, gap, actualWidth, actualHeight));

                    RenderText(context, foregroundColor, instance, instance.PrimaryKey, instance.PrimaryBounds);
                    RenderText(context, foregroundColor, instance, instance.SecondaryKey, instance.SecondaryBounds, false);

                    context.Pop();
                }
            }
        }
        
        /// <summary>
        /// Renders a <see cref="KeyGeometryBuilder"/> text within a rectangle container.
        /// </summary>
        /// <param name="context">The drawing context.</param>
        /// <param name="color">The text color brush.</param>
        /// <param name="instance">The <see cref="KeyInstance"/> which is being rendered.</param>
        /// <param name="keyGeometryBuilder">The <see cref="KeyGeometryBuilder"/> which holds the <see cref="FormattedText"/>.</param>
        /// <param name="containerBounds">The bounds of the text.</param>
        /// <param name="centered">Determines whether or not the text should be centered around the x axis.</param>
        private void RenderText(DrawingContext context, SolidColorBrush color, KeyInstance instance, KeyGeometryBuilder keyGeometryBuilder, Rect containerBounds, bool centered = true)
        {
            var formattedText = keyGeometryBuilder.FormattedText;
            if (formattedText != null)
            {
                var fontHeight = Math.Max(containerBounds.Height, 1);
                var fontSize = PixelToPoint(fontHeight);
                formattedText.SetFontSize(fontSize);
                formattedText.SetForegroundBrush(color);
                var textWidth = formattedText.Width;
                if (textWidth > 0 && textWidth > containerBounds.Width)
                {
                    var widthRatio = containerBounds.Width / textWidth;
                    fontSize *= widthRatio;
                    fontHeight *= widthRatio;
                    formattedText.SetFontSize(Math.Max(fontSize, 1));
                    textWidth = formattedText.Width;
                }
                double offsetX = containerBounds.X;
                double offsetY = containerBounds.Y + (containerBounds.Height - fontHeight) / 2;
                if (centered)
                {
                    offsetX += (containerBounds.Width - textWidth) / 2;
                }
                
                if (KeyboardDebug.DebugKeyTextLayout)
                {
                    context.DrawRectangle(null, new Pen(Brushes.Green, 1.0), new Rect(0, 0, instance.Bounds.Width, instance.Bounds.Height));
                    context.DrawRectangle(null, new Pen(Brushes.Yellow, 1.0), containerBounds);
                }
                context.DrawText(formattedText, new Point(offsetX, offsetY));
            }
        }

        /// <summary>
        /// Converts pixel to points.
        /// Used to convert pixel into font size.
        /// </summary>
        public double PixelToPoint(double pixels)
        {
            return pixels / 1.33333;
        }

        /// <summary>
        /// Converts points to pixels.
        /// Used to convert font size into pixel.
        /// </summary>
        public double PointToPixel(double points)
        {
            return points * 1.33333;
        }
    }
}
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
        private static SolidColorBrush focusBackgroundBrush = new SolidColorBrush(Colors.Fuchsia);
        private static SolidColorBrush focusForegroundBrush = new SolidColorBrush(Colors.Fuchsia);

        public void BeforeRender(Keyboard keyboard)
        {
            focusBackgroundBrush.Color = keyboard.Theme.GetColor(ThemeColor.KeyDownBackground);
            focusForegroundBrush.Color = keyboard.Theme.GetColor(ThemeColor.KeyDownForeground);
        }
        
        public void Render(Keyboard keyboard, DrawingContext c, KeyInstance instance, IKeyGroupRenderer renderer)
        {
            if (instance.Settings.IsVisible)
            {
                var gap = keyboard.KeyGap;
                double actualWidth = instance.Bounds.Width - gap * 2;
                double actualHeight = instance.Bounds.Height - gap * 2;
                if (actualWidth > 0 && actualHeight > 0)
                {
                    c.PushTransform(new TranslateTransform(instance.Bounds.X, instance.Bounds.Y));

                    var backgroundColor = instance.BackgroundBrush;
                    var foregroundColor = instance.ForegroundBrush;

                    var role = instance.Settings.KnownRole;
                    if ((role != KeyRole.Shift && keyboard.IsModifierActive(role)) || 
                        (role == KeyRole.Shift && WinApiHelper.IsCapsLock))
                    {
                        backgroundColor = focusBackgroundBrush;
                        foregroundColor = focusForegroundBrush;
                    }
                    
                    c.DrawRectangle(backgroundColor, null, new Rect(gap, gap, actualWidth, actualHeight));

                    RenderText(c, foregroundColor, instance, instance.PrimaryKey, instance.PrimaryBounds);
                    RenderText(c, foregroundColor, instance, instance.SecondaryKey, instance.SecondaryBounds, false);

                    c.Pop();
                }
            }
        }
        
        private void RenderText(DrawingContext c, SolidColorBrush color, KeyInstance instance, KeyGeometryBuilder keyGeometryBuilder, Rect containerBounds, bool centered = true)
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
                    c.DrawRectangle(null, new Pen(Brushes.Green, 1.0), new Rect(0, 0, instance.Bounds.Width, instance.Bounds.Height));
                    c.DrawRectangle(null, new Pen(Brushes.Yellow, 1.0), containerBounds);
                }
                c.DrawText(formattedText, new Point(offsetX, offsetY));
            }
        }

        public double PixelToPoint(double pixels)
        {
            return pixels / 1.33333;
        }

        public double PointToPixel(double points)
        {
            return points * 1.33333;
        }
    }
}
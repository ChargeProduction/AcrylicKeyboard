using System.Windows;

namespace AcrylicKeyboard.Layout.Sizes
{
    public interface IKeyboardSizeResolver
    {
        (Rect bounds, int gap) ResolveSize(Size sourceSize);
    }
}
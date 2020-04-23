using System.Windows;

namespace AcrylicKeyboard.Layout.Sizes
{
    public interface IKeyboardSizeResolver
    {
        /// <summary>
        /// Recalculates bounds and key gap using a specified size.
        /// </summary>
        /// <param name="sourceSize">The specified size.</param>
        /// <returns>The new bounds and key gap.</returns>
        (Rect bounds, int gap) ResolveSize(Size sourceSize);
    }
}
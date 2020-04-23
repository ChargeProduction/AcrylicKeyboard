using System.Windows;
using System.Windows.Media;
using AcrylicKeyboard.Layout;

namespace AcrylicKeyboard.Renderer
{
    public interface IKeyGroupRenderer
    {
        /// <summary>
        /// Update function.
        /// </summary>
        /// <param name="delta">The passed time in second from last to current frame.</param>
        void OnUpdate(double delta);
        
        /// <summary>
        /// Render function.
        /// </summary>
        /// <param name="context">The context to draw to.</param>
        void OnRender(DrawingContext context);
        
        /// <summary>
        /// Returns a key at the given x and y pixel coordinate.
        /// </summary>
        KeyInstance GetKeyAt(int x, int y);
        
        /// <summary>
        /// Gets the renderer bounds.
        /// </summary>
        Rect Bounds { get; }
    }
}
using System.Windows;
using System.Windows.Media;
using AcrylicKeyboard.Layout;

namespace AcrylicKeyboard.Renderer
{
    public interface IKeyGroupRenderer
    {
        void OnRender(DrawingContext c);
        
        KeyInstance GetKeyAt(int x, int y);
        
        Rect Bounds { get; }
    }
}
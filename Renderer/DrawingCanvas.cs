using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace AcrylicKeyboard.Renderer
{
    public class DrawingCanvas : Canvas, IDisposable
    {
        private Action<DrawingContext> onRender;
        private DrawingGroup backingStore = new DrawingGroup();

        public DrawingCanvas(Action<DrawingContext> onRender)
        {
            this.onRender = onRender;
            CompositionTarget.Rendering += OnRenderFrame;
        }

        private void OnRenderFrame(object sender, EventArgs e)
        {
            RenderFrame();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            RenderFrame();
            dc.DrawDrawing(backingStore);
        }

        private void RenderFrame()
        {
            var dc = backingStore.Open();
            onRender?.Invoke(dc);
            dc.Close();
        }

        public void Dispose()
        {
            CompositionTarget.Rendering -= OnRenderFrame;
        }
    }
}
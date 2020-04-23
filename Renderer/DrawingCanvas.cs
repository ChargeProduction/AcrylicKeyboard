using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using BitfinexTrader;

namespace AcrylicKeyboard.Renderer
{
    public class DrawingCanvas : Panel, IDisposable
    {
        private double fps;
        private readonly MovingAverage fpsMovingAverage = new MovingAverage(60);
        private double cycleTime;
        private Stopwatch watch;
        private readonly Action<double> onUpdate;
        private readonly Action<DrawingContext> onRender;
        private readonly DrawingGroup backingStore = new DrawingGroup();
        private bool isInvalidated = true;

        /// <param name="onUpdate">Callback on update</param>
        /// <param name="onRender">Callback on render</param>
        public DrawingCanvas(Action<double> onUpdate, Action<DrawingContext> onRender)
        {
            this.onUpdate = onUpdate;
            this.onRender = onRender;
            CompositionTarget.Rendering += OnRenderFrame;
        }

        private void OnRenderFrame(object sender, EventArgs e)
        {
            if (watch == null)
            {
                watch = new Stopwatch();
                watch.Start();
            }
            else
            {
                var delta = watch.Elapsed.TotalMilliseconds / 1000.0;
                watch.Reset();
                watch.Start();

                fps = 1 / delta;
                fpsMovingAverage.Push(fps);
                onUpdate?.Invoke(delta);
                RenderFrame();
                cycleTime = watch.Elapsed.TotalMilliseconds / 1000.0;
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            RenderFrame();
            dc.DrawDrawing(backingStore);
        }

        /// <summary>
        ///     If the renderer is invalidated, the frame is being rendered into the backing store.
        /// </summary>
        private void RenderFrame()
        {
            if (isInvalidated)
            {
                var dc = backingStore.Open();
                onRender?.Invoke(dc);
                dc.Close();
                isInvalidated = false;
            }
        }

        /// <summary>
        ///     Invalidates the renderer and forces redraw.
        /// </summary>
        internal void InvalidateRender()
        {
            isInvalidated = true;
        }

        public void Dispose()
        {
            CompositionTarget.Rendering -= OnRenderFrame;
        }

        /// <summary>
        ///     Gets the current frames per second.
        /// </summary>
        public double Fps => fps;

        /// <summary>
        ///     Gets an average frames per second of the last 60 probes.
        /// </summary>
        public double SmoothFps => fpsMovingAverage.Value;

        /// <summary>
        ///     Gets the last cycle time which only includes updating and rendering.
        /// </summary>
        public double CycleTime => cycleTime;
    }
}
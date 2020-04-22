using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using GlmSharp;

namespace AcrylicKeyboard.Renderer.Animation
{
    public class TransformAnimation
    {
        public delegate void OnFinishDelegate(TransformAnimation sender);
        
        private List<TransformFrame> frames = new List<TransformFrame>();
        private Stopwatch watch;
        private int currentIndex = 0;
        private double time;
        private double timeOffset;
        private double totalDuration;
        private bool hasStarted;
        private bool hasFinished;
        private TransformFrame currentFrame;
        private Easings.EasingFunction easingFunction = Easings.EasingFunction.CubicEaseInOut;

        private Stack<int> pushedTransforms = new Stack<int>();

        public event OnFinishDelegate OnFinish;

        public void AddFrame(TransformFrame frame)
        {
            frames.Add(frame);
            UpdateTotalDuration();
        }

        public void AdjustFrame(int index, Func<Builder, Builder> func)
        {
            if (index >= 0 && index < frames.Count)
            {
                var builder = new Builder();
                builder.Result = frames[index];
                frames[index] = func(builder).Result;
                UpdateTotalDuration();
            }
        }

        public void AddFrame(ITransformBuilder builder)
        {
            if (builder != null)
            {
                frames.AddRange(builder.GetFrames());
                UpdateTotalDuration();
            }
        }

        public void AddFrames(ITransformBuilder builder)
        {
            if (builder != null)
            {
                frames.AddRange(builder.GetFrames());
                UpdateTotalDuration();
            }
        }

        public void PushTransform(DrawingContext context)
        {
            int transformCounter = 0;
            if (CurrentFrame.Position.LengthSqr > 0)
            {
                context.PushTransform(new TranslateTransform(CurrentFrame.Position.x, CurrentFrame.Position.y));
            }
            if (CurrentFrame.AngleDeg != 0)
            {
                context.PushTransform(new RotateTransform(CurrentFrame.AngleDeg));
            }
            if (CurrentFrame.Scale.x != 1 && CurrentFrame.Scale.y != 1)
            {
                context.PushTransform(new ScaleTransform(CurrentFrame.Scale.x, CurrentFrame.Scale.y));
            }
            pushedTransforms.Push(transformCounter);
        }

        public void Pop(DrawingContext context)
        {
            if (pushedTransforms.Count > 0)
            {
                int transformCounter = pushedTransforms.Pop();
                for (int i = 0; i < transformCounter; i++)
                {
                    context.Pop();
                }
            }
        }

        private void UpdateTotalDuration()
        {
            totalDuration = 0;
            for (var i = 0; i < frames.Count - 1; i++)
            {
                totalDuration += frames[i].Duration;
            }
        }

        public void Start()
        {
            watch = new Stopwatch();
            watch.Start();
            currentIndex = 0;
            time = 0;
            timeOffset = 0;
            hasStarted = true;
            hasFinished = false;
        }

        public void End()
        {
            watch.Stop();
            if (frames.Count > 0)
            {
                currentFrame = frames[frames.Count - 1];
            }
            hasFinished = true;
            OnFinish?.Invoke(this);
        }
        
        public void Update()
        {
            if (hasStarted && frames.Count > 0 && !hasFinished)
            {
                time = watch.ElapsedMilliseconds;
                TransformFrame frame;
                while (currentIndex < frames.Count - 1 && time > (frame = frames[currentIndex]).Duration + timeOffset)
                {
                    timeOffset += frame.Duration;
                    currentIndex++;
                }

                if (currentIndex == frames.Count - 1)
                {
                    End();
                }
                else
                {
                    var from = frames[currentIndex];
                    var to = frames[Math.Min(frames.Count - 1, currentIndex + 1)];
                    double step = Math.Max(0, Math.Min(1, time / totalDuration));
                    currentFrame = from.InterpolateTo(to, step, easingFunction);
                }
            }
        }

        public TransformFrame CurrentFrame => currentFrame;

        public IReadOnlyList<TransformFrame> Frames => frames;

        public int CurrentIndex => currentIndex;

        public double Time => time;

        public bool HasStarted => hasStarted;

        public bool HasFinished => hasFinished;

        public Easings.EasingFunction EasingFunction
        {
            get => easingFunction;
            set => easingFunction = value;
        }

        public struct Builder : ITransformBuilder
        {
            public TransformFrame Result;

            public Builder WithPosition(double x, double y)
            {
                return WithPosition(new dvec2(x, y));
            }

            public Builder WithPosition(dvec2 position)
            {
                Result.Position = position;
                return this;
            }

            public Builder WithScale(double scale)
            {
                return WithScale(new dvec2(scale, scale));
            }

            public Builder WithScale(double width, double height)
            {
                return WithScale(new dvec2(width, height));
            }

            public Builder WithScale(dvec2 scale)
            {
                Result.Scale = scale;
                return this;
            }

            public Builder WithAngle(double angleDeg)
            {
                Result.AngleDeg = angleDeg;
                return this;
            }

            public Builder WithDuration(double duration)
            {
                Result.Duration = duration;
                return this;
            }

            public List<TransformFrame> GetFrames()
            {
                return new List<TransformFrame> { Result };
            }
        }
    }
}
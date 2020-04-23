using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using GlmSharp;

namespace AcrylicKeyboard.Renderer.Animation
{
    public class TransformAnimation
    {
        public delegate void ModifyTransformFrameDelegate(ref Builder builder);
        public delegate void OnFinishDelegate(TransformAnimation sender);
        
        private List<TransformFrame> frames = new List<TransformFrame>();
        private Stack<int> pushedTransforms = new Stack<int>();
        
        private int currentIndex;
        private double time;
        private double interpolatedTime;
        private double timeOffset;
        private double totalDuration;
        private bool hasStarted;
        private bool hasFinished;
        private int maxIterations = 1;
        private int iterationCount;
        private TransformFrame currentFrame;
        private Easings.EasingFunction easingFunction = Easings.EasingFunction.CubicEaseInOut;

        public event OnFinishDelegate OnFinish;

        /// <summary>
        /// Adds a frame to the end of the animation.
        /// </summary>
        /// <param name="frame">The <see cref="TransformFrame"/> to add.</param>
        public void AddFrame(TransformFrame frame)
        {
            frames.Add(frame);
            UpdateTotalDuration();
        }

        /// <summary>
        /// Adjusts the <see cref="TransformFrame"/> of the specified index using a builder.
        /// </summary>
        /// <param name="index">The index to adjust.</param>
        /// <param name="index">The function which performs the adjustment.</param>
        public void AdjustFrame(int index, ModifyTransformFrameDelegate func)
        {
            if (index >= 0 && index < frames.Count)
            {
                var builder = new Builder();
                builder.Result = frames[index];
                func(ref builder);
                frames[index] = builder.Result;
                UpdateTotalDuration();
            }
        }

        /// <summary>
        /// Adds a list of <see cref="TransformFrame"/> items from a specified <see cref="ITransformBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ITransformBuilder"/> which stores the <see cref="TransformFrame"/> items.</param>
        public void AddFrames(ITransformBuilder builder)
        {
            if (builder != null)
            {
                frames.AddRange(builder.GetFrames());
                UpdateTotalDuration();
            }
        }

        /// <summary>
        /// Pushes the current frame to the specified <see cref="DrawingContext"/> and stores the information of this action.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> which should push the transforms.</param>
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

        /// <summary>
        /// Pops all transformations which were stored in <see cref="PushTransform"/>.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> which should pop the transforms.</param>
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

        /// <summary>
        /// Updates the total animation duration.
        /// </summary>
        private void UpdateTotalDuration()
        {
            totalDuration = 0;
            for (var i = 0; i < frames.Count - 1; i++)
            {
                totalDuration += frames[i].Duration;
            }
        }

        /// <summary>
        /// Starts the animation.
        /// </summary>
        internal void Start()
        {
            currentIndex = 0;
            time = 0;
            timeOffset = 0;
            hasStarted = true;
            hasFinished = false;
            currentFrame = frames.FirstOrDefault();
        }

        /// <summary>
        /// Cleans up after the animation has ended or should end.
        /// </summary>
        internal void End()
        {
            iterationCount = 0;
            currentFrame = frames.LastOrDefault();
            hasFinished = true;
            hasStarted = false;
            OnFinish?.Invoke(this);
        }
        
        /// <summary>
        /// Updates the animation by a given timestep.
        /// Current time value is offset by the timestep and interpolated using the given equation.
        /// Then the next frame is queried which may skip some frames in between.
        /// Finally the current and next <see cref="TransformFrame"/> are interpolated and 
        /// the <see cref="CurrentFrame"/> is updated.
        /// </summary>
        /// <param name="delta"></param>
        internal void Update(double delta)
        {
            if (hasStarted && frames.Count > 0 && totalDuration > 0 && !hasFinished)
            {
                time += delta;
                interpolatedTime = Easings.Interpolate(time / totalDuration, EasingFunction) * totalDuration;
                TransformFrame frame;
                while (currentIndex < frames.Count - 1 && interpolatedTime > (frame = frames[currentIndex]).Duration + timeOffset)
                {
                    timeOffset += frame.Duration;
                    currentIndex++;
                }

                if (currentIndex == frames.Count - 1)
                {
                    iterationCount++;
                    if (maxIterations > 0 && iterationCount >= maxIterations)
                    {
                        End();
                    }
                    else
                    {
                        Start();
                    }
                }
                else
                {
                    var from = frames[currentIndex];
                    var to = frames[Math.Min(frames.Count - 1, currentIndex + 1)];
                    currentFrame = from.LerpTo(to, interpolatedTime);
                }
            }
        }

        /// <summary>
        /// Gets the current interpolated frame.
        /// </summary>
        public TransformFrame CurrentFrame => currentFrame;

        /// <summary>
        /// Gets a list of all frames of this animations
        /// </summary>
        public IReadOnlyList<TransformFrame> Frames => frames;

        /// <summary>
        /// Gets the current index of the frame.
        /// </summary>
        public int CurrentIndex => currentIndex;

        /// <summary>
        /// Gets the current animation time in seconds.
        /// </summary>
        public double Time => time;

        /// <summary>
        /// Gets the total animation time in seconds;
        /// </summary>
        public double TotalDuration => totalDuration;

        /// <summary>
        /// Determines whether or not the animation has started.
        /// </summary>
        public bool HasStarted => hasStarted;

        /// <summary>
        /// Determines whether or not the animation has finished.
        /// </summary>
        public bool HasFinished => hasFinished;
        
        /// <summary>
        /// Gets or sets the maximum amount of iterations. 0 or below is infinite.
        /// </summary>
        public int MaxIterations
        {
            get => maxIterations;
            set => maxIterations = value;
        }
        
        /// <summary>
        /// Gets or sets the easing function for the whole animation timeline.
        /// </summary>
        public Easings.EasingFunction EasingFunction
        {
            get => easingFunction;
            set => easingFunction = value;
        }

        /// <summary>
        /// Creates a new animation builder.
        /// </summary>
        /// <returns></returns>
        public Builder NewBuilder()
        {
            return new Builder();
        }

        public struct Builder : ITransformBuilder
        {
            public TransformFrame Result;

            /// <summary>
            /// Sets the position of the transform.
            /// </summary>
            public Builder WithPosition(double x, double y)
            {
                return WithPosition(new dvec2(x, y));
            }

            /// <summary>
            /// Sets the position of the transform.
            /// </summary>
            public Builder WithPosition(dvec2 position)
            {
                Result.Position = position;
                return this;
            }

            /// <summary>
            /// Sets the scale of the transform.
            /// </summary>
            public Builder WithScale(double scale)
            {
                return WithScale(new dvec2(scale, scale));
            }

            /// <summary>
            /// Sets the scale of the transform.
            /// </summary>
            public Builder WithScale(double width, double height)
            {
                return WithScale(new dvec2(width, height));
            }

            /// <summary>
            /// Sets the scale of the transform.
            /// </summary>
            public Builder WithScale(dvec2 scale)
            {
                Result.Scale = scale;
                return this;
            }

            /// <summary>
            /// Sets the rotation angle by degrees of the transform.
            /// </summary>
            public Builder WithAngle(double angleDeg)
            {
                Result.AngleDeg = angleDeg;
                return this;
            }

            /// <summary>
            /// Sets the duration in seconds of the transform.
            /// </summary>
            public Builder WithDuration(double duration)
            {
                Result.Duration = duration;
                return this;
            }

            /// <summary>
            /// Returns all frames as a list.
            /// </summary>
            public List<TransformFrame> GetFrames()
            {
                return new List<TransformFrame> { Result };
            }
        }
    }
}
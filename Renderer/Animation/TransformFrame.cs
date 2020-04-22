using GlmSharp;

namespace AcrylicKeyboard.Renderer.Animation
{
    public struct TransformFrame
    {
        public dvec2 Position;
        public dvec2 Scale;
        public double AngleDeg;
        public double Duration;

        public TransformFrame InterpolateFrom(TransformFrame frame, double step, Easings.EasingFunction function)
        {
            return Interpolate(frame, this, step, function);
        }

        public TransformFrame InterpolateTo(TransformFrame frame, double step, Easings.EasingFunction function)
        {
            return Interpolate(this, frame, step, function);
        }

        private TransformFrame Interpolate(TransformFrame from, TransformFrame to, double step, Easings.EasingFunction function)
        {
            var result = default(TransformFrame);
            double interpolatedValue = Easings.Interpolate(step, function);
            result.Position = dvec2.Lerp(from.Position, to.Position, interpolatedValue);
            result.Scale = dvec2.Lerp(from.Scale, to.Scale, interpolatedValue);
            result.AngleDeg = from.AngleDeg * (1.0 - interpolatedValue) + to.AngleDeg * interpolatedValue;
            return result;
        }
    }
}
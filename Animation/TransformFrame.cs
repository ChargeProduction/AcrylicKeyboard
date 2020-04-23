using GlmSharp;

namespace AcrylicKeyboard.Renderer.Animation
{
    public struct TransformFrame
    {
        public dvec2 Position;
        public dvec2 Scale;
        public double AngleDeg;
        public double Duration;

        /// <summary>
        /// Linear interpolation from a given <see cref="TransformFrame"/> to this.
        /// </summary>
        /// <param name="frame">The <see cref="TransformFrame"/> to interpolate from.</param>
        /// <param name="step">The interpolation step from 0.0 to 1.0.</param>
        /// <returns>The interpolated <see cref="TransformFrame"/></returns>
        public TransformFrame LerpFrom(TransformFrame frame, double step)
        {
            return Interpolate(frame, this, step);
        }

        /// <summary>
        /// Linear interpolation to a given <see cref="TransformFrame"/> to this.
        /// </summary>
        /// <param name="frame">The <see cref="TransformFrame"/> to interpolate to.</param>
        /// <param name="step">The interpolation step from 0.0 to 1.0.</param>
        /// <returns>The interpolated <see cref="TransformFrame"/></returns>
        public TransformFrame LerpTo(TransformFrame frame, double step)
        {
            return Interpolate(this, frame, step);
        }

        /// <summary>
        /// Performs the interpolation.
        /// </summary>
        /// <param name="from">The <see cref="TransformFrame"/> to interpolate from.</param>
        /// <param name="to">The <see cref="TransformFrame"/> to interpolate to.</param>
        /// <param name="step">The interpolation step from 0.0 to 1.0.</param>
        /// <returns></returns>
        private TransformFrame Interpolate(TransformFrame from, TransformFrame to, double step)
        {
            var result = default(TransformFrame);
            result.Position = dvec2.Lerp(from.Position, to.Position, step);
            result.Scale = dvec2.Lerp(from.Scale, to.Scale, step);
            result.AngleDeg = from.AngleDeg * (1.0 - step) + to.AngleDeg * step;
            return result;
        }
    }
}
using System.Collections.Generic;

namespace AcrylicKeyboard.Renderer.Animation
{
    public interface ITransformBuilder
    {
        /// <summary>
        /// Returns all frames of this builder.
        /// </summary>
        /// <returns>List of <see cref="TransformFrame"/> items.</returns>
        List<TransformFrame> GetFrames();
    }
}
using System.Collections.Generic;

namespace AcrylicKeyboard.Renderer.Animation
{
    public interface ITransformBuilder
    {
        List<TransformFrame> GetFrames();
    }
}
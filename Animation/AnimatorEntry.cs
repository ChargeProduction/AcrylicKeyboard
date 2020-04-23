using System;

namespace AcrylicKeyboard.Renderer.Animation
{
    public class AnimatorEntry
    {
        private TransformAnimation animation;
        private Action<Animator, TransformAnimation> onUpdate;

        public AnimatorEntry(TransformAnimation animation, Action<Animator, TransformAnimation> onUpdate)
        {
            this.animation = animation;
            this.onUpdate = onUpdate;
        }

        /// <summary>
        /// The stored animation.
        /// </summary>
        public TransformAnimation Animation => animation;

        /// <summary>
        /// The stored update callback action.
        /// </summary>
        public Action<Animator, TransformAnimation> OnUpdate => onUpdate;
    }
}
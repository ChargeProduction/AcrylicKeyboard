using System;
using System.Collections.Generic;
using System.Linq;

namespace AcrylicKeyboard.Renderer.Animation
{
    public class Animator
    {
        private List<AnimatorEntry> runningAnimations = new List<AnimatorEntry>();
        private List<AnimatorEntry> activeAnimations = new List<AnimatorEntry>();

        /// <summary>
        ///     Updates all running animations.
        /// </summary>
        /// <param name="delta">The time difference from the previous frame in seconds.</param>
        internal void Update(double delta)
        {
            for (var i = 0; i < runningAnimations.Count; i++)
            {
                var entry = runningAnimations[i];
                entry.Animation.Update(delta);
                entry.OnUpdate?.Invoke(this, entry.Animation);
                if (!entry.Animation.HasFinished)
                {
                    activeAnimations.Add(entry);
                }
            }

            // Swapping lists to prevent item shifting inside the list
            var tmpRunningAnimations = runningAnimations;
            runningAnimations = activeAnimations;
            activeAnimations = tmpRunningAnimations;
        }

        /// <summary>
        ///     Plays the given animation.
        /// </summary>
        /// <param name="animation">The animation to play.</param>
        /// <param name="onUpdateCallback">An optional callback action which is called on update.</param>
        public void Play(TransformAnimation animation, Action<Animator, TransformAnimation> onUpdateCallback = null)
        {
            if (animation.HasStarted)
            {
                animation.End();
            }

            runningAnimations.Add(new AnimatorEntry(animation, onUpdateCallback));
            animation.Start();
        }

        /// <summary>
        ///     Returns an enumerable copy of all running animations.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TransformAnimation> GetRunningAnimations()
        {
            return runningAnimations.Select(entry => entry.Animation);
        }
    }
}
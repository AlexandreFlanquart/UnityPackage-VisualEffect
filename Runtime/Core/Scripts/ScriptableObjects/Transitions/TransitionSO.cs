using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Base class for all transition scripts.
    /// </summary>
    public abstract class TransitionSO : ScriptableObject
    {
        [SerializeField] private string _transitionName;
        public string transitionName => _transitionName;

        /// <summary>
        /// Plays the transition on <paramref name="target"/>. The returned task completes when the
        /// transition is finished, or early if the target is destroyed or Play Mode is exited.
        /// </summary>
        public abstract Task PlayTransition(GameObject target);

        /// <summary>
        /// Runs <paramref name="onProgress"/> every frame with a 0..1 progress value for <paramref name="duration"/> seconds.
        /// Always ends with a call at exactly 1, unless interrupted.
        /// </summary>
        /// <returns>False if the target was destroyed or Play Mode was exited before the end.</returns>
        protected static async Task<bool> RunOverTime(GameObject target, float duration, bool unscaledTime, System.Action<float> onProgress = null)
        {
            CancellationToken token = Application.exitCancellationToken;
            float elapsed = 0f;

            try
            {
                while (elapsed < duration)
                {
                    onProgress?.Invoke(elapsed / duration);
                    await Awaitable.NextFrameAsync(token);

                    // Unity null check: the target may have been destroyed while we were waiting
                    if (target == null) return false;

                    elapsed += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                }
            }
            catch (System.OperationCanceledException)
            {
                return false;
            }

            onProgress?.Invoke(1f);
            return true;
        }
    }
}

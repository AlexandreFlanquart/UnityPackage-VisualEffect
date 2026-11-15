using System.Threading.Tasks;
using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Fade transition by script, driven through a CanvasGroup (added to the target if missing).
    /// </summary>
    [CreateAssetMenu(fileName = "FadeTransitionSO", menuName = "MyUnityPackage/Visual Effect/Transitions/Custom Fade")]
    public class CustomFadeTransitionSO : CustomTransitionSO
    {
        public enum FadeMode
        {
            /// <summary>Fades to transparent, then back to opaque (2 x fadeDuration).</summary>
            OutThenIn = 0,
            /// <summary>Transparent to opaque.</summary>
            In = 1,
            /// <summary>Opaque to transparent.</summary>
            Out = 2
        }

        [Tooltip("Duration of one fade (seconds)")]
        [Min(0f)] public float fadeDuration = 1f;

        public FadeMode mode = FadeMode.OutThenIn;

        [Tooltip("Ignore Time.timeScale, so the fade still plays while the game is paused")]
        public bool useUnscaledTime = true;

        protected override async Task OnCustomTransition(GameObject target)
        {
            if (!target.TryGetComponent(out CanvasGroup canvasGroup))
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
            }

            if (mode != FadeMode.In)
            {
                if (!await Fade(target, canvasGroup, 1f, 0f)) return;
            }

            if (mode != FadeMode.Out)
            {
                await Fade(target, canvasGroup, 0f, 1f);
            }
        }

        private Task<bool> Fade(GameObject target, CanvasGroup canvasGroup, float from, float to)
        {
            // RunOverTime always finishes on progress = 1, so the final alpha is exact even with a zero duration
            return RunOverTime(target, fadeDuration, useUnscaledTime, progress =>
            {
                if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(from, to, progress);
            });
        }
    }
}

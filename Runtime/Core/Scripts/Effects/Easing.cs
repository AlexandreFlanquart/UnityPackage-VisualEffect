using System;
using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    public enum Ease
    {
        Linear,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InBack,
        OutBack,
        OutElastic,
        OutBounce,
        /// <summary>Use the AnimationCurve of the <see cref="EaseSetting"/>.</summary>
        Custom
    }

    /// <summary>
    /// Ease choice shown in the Inspector: a preset, or a custom curve when <see cref="Ease.Custom"/> is selected.
    /// </summary>
    [Serializable]
    public class EaseSetting
    {
        [SerializeField] private Ease _ease = Ease.OutQuad;
        [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public EaseSetting() { }
        public EaseSetting(Ease ease) { _ease = ease; }

        public float Evaluate(float t)
        {
            return _ease == Ease.Custom ? _curve.Evaluate(t) : Easing.Evaluate(_ease, t);
        }
    }

    public static class Easing
    {
        private const float BackOvershoot = 1.70158f;

        /// <summary>Maps a linear 0..1 value to an eased value (0 at t = 0, 1 at t = 1; Back and Elastic overshoot).</summary>
        public static float Evaluate(Ease ease, float t)
        {
            t = Mathf.Clamp01(t);

            switch (ease)
            {
                case Ease.InQuad: return t * t;
                case Ease.OutQuad: return 1f - (1f - t) * (1f - t);
                case Ease.InOutQuad: return t < 0.5f ? 2f * t * t : 1f - 2f * (1f - t) * (1f - t);
                case Ease.InCubic: return t * t * t;
                case Ease.OutCubic: return 1f - Mathf.Pow(1f - t, 3f);
                case Ease.InOutCubic: return t < 0.5f ? 4f * t * t * t : 1f - 4f * Mathf.Pow(1f - t, 3f);
                case Ease.InBack: return (BackOvershoot + 1f) * t * t * t - BackOvershoot * t * t;
                case Ease.OutBack:
                {
                    float u = t - 1f;
                    return 1f + (BackOvershoot + 1f) * u * u * u + BackOvershoot * u * u;
                }
                case Ease.OutElastic:
                {
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * (2f * Mathf.PI / 3f)) + 1f;
                }
                case Ease.OutBounce: return OutBounce(t);
                default: return t; // Linear, and Custom (handled by EaseSetting)
            }
        }

        private static float OutBounce(float t)
        {
            const float n = 7.5625f;
            const float d = 2.75f;

            if (t < 1f / d) return n * t * t;
            if (t < 2f / d) { t -= 1.5f / d; return n * t * t + 0.75f; }
            if (t < 2.5f / d) { t -= 2.25f / d; return n * t * t + 0.9375f; }
            t -= 2.625f / d;
            return n * t * t + 0.984375f;
        }
    }
}

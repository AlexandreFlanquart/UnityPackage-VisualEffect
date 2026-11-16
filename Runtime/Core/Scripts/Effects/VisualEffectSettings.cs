using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Global accessibility settings, to wire to the game's options menu ("reduce motion", "reduce flashing").
    /// They only attenuate decorative effects: an effect that carries meaning (a panel scaling in, a fade)
    /// always reaches its end state.
    /// </summary>
    public static class VisualEffectSettings
    {
        private static float _motionScale = 1f;
        private static float _flashScale = 1f;

        /// <summary>
        /// Amplitude multiplier of decorative motion (punch, pulse, shake, squash, float, linear move).
        /// 1 = full motion, 0 = none.
        /// </summary>
        public static float MotionScale
        {
            get => _motionScale;
            set => _motionScale = Mathf.Clamp01(value);
        }

        /// <summary>Intensity multiplier of colour flashes (HitFlashEffect). 1 = full flash, 0 = none.</summary>
        public static float FlashScale
        {
            get => _flashScale;
            set => _flashScale = Mathf.Clamp01(value);
        }

        // Static state survives Play Mode sessions when Domain Reload is disabled: start each session from defaults.
        // The game is expected to apply the player's saved preference at startup.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _motionScale = 1f;
            _flashScale = 1f;
        }
    }
}

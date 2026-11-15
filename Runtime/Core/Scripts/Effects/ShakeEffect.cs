using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// One-shot random shake that fades out. Typical uses: camera shake, damage feedback.
    /// </summary>
    [AddComponentMenu("MyUnityPackage/Visual Effect/Shake Effect")]
    public class ShakeEffect : VisualEffectBehaviour
    {
        [Header("Shake")]
        [Tooltip("Transform to animate (defaults to this object)")]
        [SerializeField] private Transform _target;

        [SerializeField] private TransformChannel _channel = TransformChannel.Position;

        [Tooltip("Peak offset. Position: units, Rotation: degrees, Scale: fraction of the current scale")]
        [SerializeField] private Vector3 _strength = new Vector3(0.1f, 0.1f, 0.1f);

        [Tooltip("Noise changes per second (higher = more jittery)")]
        [Min(0f)]
        [SerializeField] private float _frequency = 25f;

        private TransformOffset _offset;

        // Different seed each play so two shakes never look identical
        private float _seed;

        public Transform Target { get => _target != null ? _target : transform; set => _target = value; }
        public TransformChannel Channel { get => _channel; set => _channel = value; }
        public Vector3 Strength { get => _strength; set => _strength = value; }
        public float Frequency { get => _frequency; set => _frequency = Mathf.Max(0f, value); }

        protected override void OnPlay()
        {
            _seed = UnityEngine.Random.value * 1000f;
        }

        protected override void Evaluate(float progress)
        {
            if (progress >= 1f)
            {
                // Exactly at rest at the end, whatever the noise says
                _offset.Apply(Target, _channel, Vector3.zero);
                return;
            }

            float time = progress * Duration * _frequency;
            float falloff = 1f - progress;

            // PerlinNoise is in [0, 1]: remap to [-1, 1]; a different row per axis decorrelates them
            Vector3 noise = new Vector3(
                Mathf.PerlinNoise(_seed, time) * 2f - 1f,
                Mathf.PerlinNoise(_seed + 17.3f, time) * 2f - 1f,
                Mathf.PerlinNoise(_seed + 41.7f, time) * 2f - 1f);

            _offset.Apply(Target, _channel, Vector3.Scale(noise, _strength) * falloff);
        }

        protected override void ResetEffect()
        {
            _offset.Clear(Target);
        }
    }
}

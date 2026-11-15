using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// One-shot impact: kicks the position, rotation or scale, then oscillates back to rest.
    /// Typical uses: button click, hit reaction, pickup collected.
    /// </summary>
    [AddComponentMenu("MyUnityPackage/Visual Effect/Punch Effect")]
    public class PunchEffect : VisualEffectBehaviour
    {
        [Header("Punch")]
        [Tooltip("Transform to animate (defaults to this object)")]
        [SerializeField] private Transform _target;

        [SerializeField] private TransformChannel _channel = TransformChannel.Scale;

        [Tooltip("Peak offset. Position: units, Rotation: degrees, Scale: fraction of the current scale")]
        [SerializeField] private Vector3 _strength = new Vector3(0.2f, 0.2f, 0.2f);

        [Tooltip("Number of back-and-forth oscillations before coming to rest")]
        [Min(1)]
        [SerializeField] private int _oscillations = 2;

        private TransformOffset _offset;

        public Transform Target { get => _target != null ? _target : transform; set => _target = value; }
        public TransformChannel Channel { get => _channel; set => _channel = value; }
        public Vector3 Strength { get => _strength; set => _strength = value; }

        protected override void Evaluate(float progress)
        {
            // Sine wave whose amplitude fades out; a whole number of periods guarantees it ends at rest
            float wave = Mathf.Sin(progress * Mathf.PI * 2f * _oscillations);
            float falloff = (1f - progress) * (1f - progress);

            _offset.Apply(Target, _channel, _strength * (wave * falloff));
        }

        protected override void ResetEffect()
        {
            _offset.Clear(Target);
        }
    }
}

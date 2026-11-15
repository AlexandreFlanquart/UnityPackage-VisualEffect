using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Continuous oscillation around the rest state (breathing button, blinking icon, hovering pickup).
    /// </summary>
    [AddComponentMenu("MyUnityPackage/Visual Effect/Pulse Effect")]
    public class PulseEffect : VisualEffectBehaviour
    {
        [Header("Pulse")]
        [Tooltip("Transform to animate (defaults to this object)")]
        [SerializeField] private Transform _target;

        [SerializeField] private TransformChannel _channel = TransformChannel.Scale;

        [Tooltip("Peak offset. Position: units, Rotation: degrees, Scale: fraction of the current scale")]
        [SerializeField] private Vector3 _amplitude = new Vector3(0.1f, 0.1f, 0.1f);

        private TransformOffset _offset;

        public Transform Target { get => _target != null ? _target : transform; set => _target = value; }
        public TransformChannel Channel { get => _channel; set => _channel = value; }
        public Vector3 Amplitude { get => _amplitude; set => _amplitude = value; }

        // Loops forever and starts by itself: a pulse is an idle effect
        public PulseEffect()
        {
            SetPlaybackDefaults(duration: 1f, loops: -1, playOnEnable: true);
        }

        protected override void Evaluate(float progress)
        {
            // One full sine period per loop, so consecutive loops join without a jump
            _offset.Apply(Target, _channel, _amplitude * Mathf.Sin(progress * Mathf.PI * 2f));
        }

        protected override void ResetEffect()
        {
            _offset.Clear(Target);
        }
    }
}

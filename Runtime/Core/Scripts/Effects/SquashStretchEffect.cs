using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    public enum SquashAxis
    {
        X,
        Y,
        Z
    }

    /// <summary>
    /// Volume-preserving squash and stretch that oscillates back to rest.
    /// Typical uses: landing, bouncing, cartoony button press.
    /// </summary>
    [AddComponentMenu("MyUnityPackage/Visual Effect/Squash Stretch Effect")]
    public class SquashStretchEffect : VisualEffectBehaviour
    {
        [Header("Squash & Stretch")]
        [Tooltip("Transform to animate (defaults to this object)")]
        [SerializeField] private Transform _target;

        [Tooltip("Axis that gets stretched (the two others compensate)")]
        [SerializeField] private SquashAxis _axis = SquashAxis.Y;

        [Tooltip("Peak deformation along the axis. Positive = stretch first, negative = squash first")]
        [Range(-0.9f, 0.9f)]
        [SerializeField] private float _amount = 0.3f;

        [Tooltip("Number of back-and-forth oscillations before coming to rest")]
        [Min(1)]
        [SerializeField] private int _oscillations = 2;

        private TransformOffset _offset;

        public Transform Target { get => _target != null ? _target : transform; set => _target = value; }
        public SquashAxis Axis { get => _axis; set => _axis = value; }
        public float Amount { get => _amount; set => _amount = Mathf.Clamp(value, -0.9f, 0.9f); }

        public SquashStretchEffect()
        {
            SetPlaybackDefaults(duration: 0.4f);
        }

        protected override void Evaluate(float progress)
        {
            float wave = Mathf.Sin(progress * Mathf.PI * 2f * _oscillations) * (1f - progress) * (1f - progress);

            // Clamped so a big amount can never flip or collapse the scale
            float along = Mathf.Max(0.05f, 1f + _amount * wave * VisualEffectSettings.MotionScale);

            // across^2 * along = 1: the volume stays constant
            float across = 1f / Mathf.Sqrt(along);

            Vector3 factor = new Vector3(
                _axis == SquashAxis.X ? along : across,
                _axis == SquashAxis.Y ? along : across,
                _axis == SquashAxis.Z ? along : across);

            _offset.Apply(Target, TransformChannel.Scale, factor - Vector3.one);
        }

        protected override void ResetEffect()
        {
            _offset.Clear(Target);
        }
    }
}

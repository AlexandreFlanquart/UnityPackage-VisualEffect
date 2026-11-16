using UnityEngine;
using UnityEngine.Serialization;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Oscillates the object back and forth along an axis (sine wave).
    /// The movement is applied as an additive offset, so it can be combined with other movers.
    /// </summary>
    [AddComponentMenu("MyUnityPackage/Visual Effect/Linear Move Object")]
    public class LinearMoveObject : MonoBehaviour
    {
        [Tooltip("Direction of movement (default is up)")]
        [FormerlySerializedAs("axis")]
        [SerializeField] private Vector3 _axis = Vector3.up;

        [Tooltip("Maximum distance from the rest position (meters)")]
        [FormerlySerializedAs("amplitude")]
        [SerializeField] private float _amplitude = 0.1f;

        [Tooltip("Oscillation frequency (cycles per second)")]
        [FormerlySerializedAs("frequency")]
        [SerializeField] private float _frequency = 1.0f;

        [Tooltip("Move in local space (true) or world space (false)")]
        [FormerlySerializedAs("localSpace")]
        [SerializeField] private bool _localSpace = true;

        [Tooltip("Use unscaled time (not affected by time scale, useful for menus or pause)")]
        [FormerlySerializedAs("useUnscaledTime")]
        [SerializeField] private bool _useUnscaledTime;

        [Tooltip("Randomize starting phase to prevent synchronized movement between objects")]
        [FormerlySerializedAs("randomStartPhase")]
        [SerializeField] private bool _randomStartPhase = true;

        // Time offset in seconds (randomized if enabled)
        private float _phase;
        private AdditiveOffset _offset;

        public Vector3 Axis { get => _axis; set => _axis = value; }
        public float Amplitude { get => _amplitude; set => _amplitude = value; }
        public float Frequency { get => _frequency; set => _frequency = value; }
        public bool LocalSpace { get => _localSpace; set => _localSpace = value; }
        public bool UseUnscaledTime { get => _useUnscaledTime; set => _useUnscaledTime = value; }

        private void Awake()
        {
            if (_randomStartPhase) _phase = Random.value * 100f;
        }

        private void Update()
        {
            float t = (_useUnscaledTime ? Time.unscaledTime : Time.time) + _phase;
            Vector3 offset = _axis.normalized * (Mathf.Sin(t * Mathf.PI * 2f * _frequency) * _amplitude * VisualEffectSettings.MotionScale);
            _offset.Apply(transform, offset, _localSpace);
        }

        private void OnDisable()
        {
            // Remove our contribution so the object rests where it would be without this effect
            _offset.Clear(transform);
        }
    }
}

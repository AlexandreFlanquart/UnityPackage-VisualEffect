using UnityEngine;
using UnityEngine.Serialization;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Idle "pickup" animation: floats along an axis with optional amplitude noise, and optionally spins.
    /// The float is applied as an additive offset, so it can be combined with other movers.
    /// </summary>
    [AddComponentMenu("MyUnityPackage/Visual Effect/Float Object")]
    public class FloatObject : MonoBehaviour
    {
        [Header("Float Settings")]
        [Tooltip("Direction of floating")]
        [FormerlySerializedAs("floatAxis")]
        [SerializeField] private Vector3 _floatAxis = Vector3.up;

        [Tooltip("Peak offset (meters)")]
        [FormerlySerializedAs("floatAmplitude")]
        [SerializeField] private float _floatAmplitude = 0.1f;

        [Tooltip("Cycles per second")]
        [FormerlySerializedAs("floatFrequency")]
        [SerializeField] private float _floatFrequency = 0.8f;

        [Tooltip("Use unscaled time (not affected by time scale, useful for menus or pause)")]
        [FormerlySerializedAs("useUnscaledTime")]
        [SerializeField] private bool _useUnscaledTime;

        [Tooltip("Move in local space (true) or world space (false)")]
        [FormerlySerializedAs("localSpace")]
        [SerializeField] private bool _localSpace = true;

        [Tooltip("Randomize starting phase to prevent synchronized movement between objects")]
        [FormerlySerializedAs("randomStartPhase")]
        [SerializeField] private bool _randomStartPhase = true;

        [Tooltip("Variation of the amplitude over time (0 = none)")]
        [FormerlySerializedAs("noise")]
        [Range(0f, 1f)]
        [SerializeField] private float _noise = 0.25f;

        [FormerlySerializedAs("noiseSpeed")]
        [SerializeField] private float _noiseSpeed = 0.25f;

        [Header("Rotation Settings")]
        [FormerlySerializedAs("enableRotation")]
        [SerializeField] private bool _enableRotation = true;

        [Tooltip("Degrees to rotate per second around each axis (X, Y, Z)")]
        [FormerlySerializedAs("degreesPerSecond")]
        [SerializeField] private Vector3 _degreesPerSecond = new Vector3(0, 30, 0);

        [FormerlySerializedAs("rotationSpace")]
        [SerializeField] private Space _rotationSpace = Space.Self;

        [Tooltip("Speed variation of the rotation (0 = constant speed)")]
        [FormerlySerializedAs("rotationJitter")]
        [Range(0f, 1f)]
        [SerializeField] private float _rotationJitter;

        // Time offset in seconds (randomized if enabled)
        private float _phase;
        private float _noiseSeed;
        private AdditiveOffset _offset;
        private Quaternion _startLocalRotation;

        public Vector3 FloatAxis { get => _floatAxis; set => _floatAxis = value; }
        public float FloatAmplitude { get => _floatAmplitude; set => _floatAmplitude = value; }
        public float FloatFrequency { get => _floatFrequency; set => _floatFrequency = value; }
        public bool UseUnscaledTime { get => _useUnscaledTime; set => _useUnscaledTime = value; }
        public bool LocalSpace { get => _localSpace; set => _localSpace = value; }
        public bool EnableRotation { get => _enableRotation; set => _enableRotation = value; }
        public Vector3 DegreesPerSecond { get => _degreesPerSecond; set => _degreesPerSecond = value; }

        private void Awake()
        {
            if (_randomStartPhase) _phase = Random.value * 100f;
            _noiseSeed = Random.value * 999f;
        }

        private void OnEnable()
        {
            // Captured on every enable so pooled or moved objects restart from their current pose
            _startLocalRotation = transform.localRotation;
        }

        private void Update()
        {
            float t = _useUnscaledTime ? Time.unscaledTime : Time.time;
            float dt = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            // Rotation
            if (_enableRotation && _degreesPerSecond.sqrMagnitude > 0f)
            {
                Vector3 d = RotationNoise.Apply(_degreesPerSecond, _rotationJitter, t + _noiseSeed);
                transform.Rotate(d * dt, _rotationSpace);
            }

            // Floating
            float perlin = (Mathf.PerlinNoise(_noiseSeed, t * _noiseSpeed) - 0.5f) * 2f;
            float amp = _floatAmplitude * (1f + perlin * _noise);
            float sine = Mathf.Sin((t + _phase) * Mathf.PI * 2f * _floatFrequency);
            _offset.Apply(transform, _floatAxis.normalized * (sine * amp), _localSpace);
        }

        private void OnDisable()
        {
            // Remove our contribution so the object rests where it would be without this effect
            _offset.Clear(transform);
            if (_enableRotation) transform.localRotation = _startLocalRotation;
        }
    }
}

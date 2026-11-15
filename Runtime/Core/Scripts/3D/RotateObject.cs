using UnityEngine;
using UnityEngine.Serialization;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Continuously rotates the object, with optional speed noise.
    /// </summary>
    [AddComponentMenu("MyUnityPackage/Visual Effect/Rotate Object")]
    public class RotateObject : MonoBehaviour
    {
        [Tooltip("Degrees to rotate per second around each axis (X, Y, Z)")]
        [FormerlySerializedAs("degreesPerSecond")]
        [SerializeField] private Vector3 _degreesPerSecond = new Vector3(0, 30, 0);

        [Tooltip("Choose between local (Self) or global (World) rotation")]
        [FormerlySerializedAs("space")]
        [SerializeField] private Space _space = Space.Self;

        [Tooltip("Use unscaled time (not affected by time scale, useful for menus or pause)")]
        [FormerlySerializedAs("useUnscaledTime")]
        [SerializeField] private bool _useUnscaledTime;

        [Tooltip("Speed variation (0 = constant speed, 1 = speed varies between 0% and 200%)")]
        [FormerlySerializedAs("noise")]
        [Range(0f, 1f)]
        [SerializeField] private float _noise;

        [Tooltip("Randomize starting phase to prevent synchronized rotation between objects")]
        [FormerlySerializedAs("randomStartPhase")]
        [SerializeField] private bool _randomStartPhase = true;

        // Time offset in seconds (randomized if enabled), used to desync the noise
        private float _phase;

        public Vector3 DegreesPerSecond { get => _degreesPerSecond; set => _degreesPerSecond = value; }
        public Space RotationSpace { get => _space; set => _space = value; }
        public bool UseUnscaledTime { get => _useUnscaledTime; set => _useUnscaledTime = value; }
        public float Noise { get => _noise; set => _noise = Mathf.Clamp01(value); }

        private void Awake()
        {
            if (_randomStartPhase) _phase = Random.value * 100f;
        }

        private void Update()
        {
            float dt = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = (_useUnscaledTime ? Time.unscaledTime : Time.time) + _phase;

            transform.Rotate(RotationNoise.Apply(_degreesPerSecond, _noise, t) * dt, _space);
        }
    }

    /// <summary>Shared Perlin-based speed variation for rotation effects.</summary>
    internal static class RotationNoise
    {
        /// <summary>Returns <paramref name="degreesPerSecond"/> scaled per axis by a factor in [1 - amount, 1 + amount].</summary>
        public static Vector3 Apply(Vector3 degreesPerSecond, float amount, float time)
        {
            if (amount <= 0f) return degreesPerSecond;

            return degreesPerSecond + new Vector3(
                (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f * degreesPerSecond.x * amount,
                (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f * degreesPerSecond.y * amount,
                (Mathf.PerlinNoise(time, time) - 0.5f) * 2f * degreesPerSecond.z * amount
            );
        }
    }
}

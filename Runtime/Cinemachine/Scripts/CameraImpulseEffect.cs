using Unity.Cinemachine;
using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Camera shake through Cinemachine Impulse, exposed with the package's effect contract so it can be
    /// triggered like any other effect (code, UnityEvent, Feel feedback).
    /// The shake itself (shape, duration, falloff) is configured on the CinemachineImpulseSource; the
    /// Cinemachine Camera must have a CinemachineImpulseListener to react to it.
    /// </summary>
    [AddComponentMenu("MyUnityPackage/Visual Effect/Camera Impulse Effect (Cinemachine)")]
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CameraImpulseEffect : MonoBehaviour, IVisualEffect
    {
        [Tooltip("Multiplier applied to the impulse source's default velocity")]
        [SerializeField] private float _force = 1f;

        [Tooltip("Send an impulse every time the component is enabled")]
        [SerializeField] private bool _playOnEnable;

        private CinemachineImpulseSource _source;
        private float _endTime;

        public float Force { get => _force; set => _force = value; }

        public float Duration => Source.ImpulseDefinition.ImpulseDuration;

        // Unscaled time: Cinemachine impulses ignore the time scale by default
        public bool IsPlaying => Application.isPlaying && Time.unscaledTime < _endTime;

        private CinemachineImpulseSource Source
        {
            get
            {
                if (_source == null) _source = GetComponent<CinemachineImpulseSource>();
                return _source;
            }
        }

        private void OnEnable()
        {
            if (_playOnEnable) Play();
        }

        public void Play()
        {
            // Impulses are simulated by Cinemachine's runtime manager: nothing to preview in Edit Mode
            if (!Application.isPlaying) return;

            Source.GenerateImpulseWithForce(_force);
            _endTime = Time.unscaledTime + Duration;
        }

        /// <summary>
        /// Cinemachine cannot cancel a single impulse (only all of them at once), so an impulse already
        /// sent finishes by itself. Stop only clears the playing state.
        /// </summary>
        public void Stop()
        {
            _endTime = 0f;
        }
    }
}

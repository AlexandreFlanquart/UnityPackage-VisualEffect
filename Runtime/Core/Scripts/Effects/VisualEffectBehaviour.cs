using UnityEngine;
using UnityEngine.Events;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Base class of time-driven effects. Handles playback (delay, duration, loops, time scale, events);
    /// subclasses only describe what the effect looks like at a given progress.
    /// </summary>
    public abstract class VisualEffectBehaviour : MonoBehaviour, IVisualEffect
    {
        public enum LoopMode
        {
            /// <summary>Each loop plays from 0 to 1.</summary>
            Restart,
            /// <summary>Loops alternate between 0 to 1 and 1 to 0.</summary>
            Yoyo
        }

        [Header("Playback")]
        [Tooltip("Play automatically every time the component is enabled")]
        [SerializeField] private bool _playOnEnable;

        [Tooltip("Duration of one loop (seconds)")]
        [Min(0f)]
        [SerializeField] private float _duration = 0.3f;

        [Tooltip("Wait before the effect starts (seconds)")]
        [Min(0f)]
        [SerializeField] private float _delay;

        [Tooltip("Number of loops (-1 = infinite)")]
        [SerializeField] private int _loops = 1;

        [SerializeField] private LoopMode _loopMode = LoopMode.Restart;

        [Tooltip("Ignore Time.timeScale (useful for menus or pause)")]
        [SerializeField] private bool _useUnscaledTime;

        [Tooltip("Invoked when the effect reaches its end (never invoked with infinite loops or when stopped)")]
        [SerializeField] private UnityEvent _onComplete = new UnityEvent();

        // Time since the end of the delay (negative during the delay)
        private float _time;

        public bool IsPlaying { get; private set; }
        public float Duration { get => _duration; set => _duration = Mathf.Max(0f, value); }
        public float Delay { get => _delay; set => _delay = Mathf.Max(0f, value); }
        public int Loops { get => _loops; set => _loops = value; }
        public LoopMode LoopType { get => _loopMode; set => _loopMode = value; }
        public bool PlayOnEnable { get => _playOnEnable; set => _playOnEnable = value; }
        public bool UseUnscaledTime { get => _useUnscaledTime; set => _useUnscaledTime = value; }
        public UnityEvent OnComplete => _onComplete;

        /// <summary>Delay plus every loop, in seconds. Infinity when the effect loops forever.</summary>
        public float TotalDuration => _loops < 0 ? float.PositiveInfinity : _delay + _duration * Mathf.Max(1, _loops);

        /// <summary>Applies the effect at <paramref name="progress"/> (0 = start of a loop, 1 = end).</summary>
        protected abstract void Evaluate(float progress);

        /// <summary>Removes everything the effect applied to its target. Must be safe to call several times in a row.</summary>
        protected abstract void ResetEffect();

        /// <summary>Called once at the beginning of <see cref="Play"/>, after the previous state has been reset.</summary>
        protected virtual void OnPlay() { }

        /// <summary>
        /// Changes the default playback values of an effect type. Call it from the subclass constructor:
        /// it then applies both to Add Component in the editor and to AddComponent at runtime,
        /// and values saved in a scene or prefab still override it.
        /// </summary>
        protected void SetPlaybackDefaults(float duration, int loops = 1, bool playOnEnable = false, LoopMode loopMode = LoopMode.Restart)
        {
            _duration = duration;
            _loops = loops;
            _playOnEnable = playOnEnable;
            _loopMode = loopMode;
        }

        protected virtual void OnEnable()
        {
            if (_playOnEnable) Play();
        }

        protected virtual void OnDisable()
        {
            Stop();
        }

        public void Play()
        {
            // Update does not run on a disabled component: the effect would freeze on its first frame
            if (Application.isPlaying && !isActiveAndEnabled) return;

            ResetEffect();

            _time = -_delay;
            IsPlaying = true;
            OnPlay();

            // Apply the first frame right away so there is no one-frame pop before the first Update
            if (_delay <= 0f) Tick(0f);
        }

        public void Stop()
        {
            IsPlaying = false;
            ResetEffect();
        }

        private void Update()
        {
            if (IsPlaying) Tick(_useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        /// <summary>
        /// Advances the effect by <paramref name="deltaTime"/> seconds. Called by Update in Play Mode;
        /// public so editor previews and tests can drive the effect without the player loop.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!IsPlaying) return;

            _time += deltaTime;
            if (_time < 0f) return; // Still in the delay

            int loops = _loops == 0 ? 1 : _loops;
            bool infinite = loops < 0;

            if (_duration <= 0f)
            {
                Complete(EndProgress(infinite ? 1 : loops));
                return;
            }

            float loopsElapsed = _time / _duration;

            if (!infinite && loopsElapsed >= loops)
            {
                Complete(EndProgress(loops));
                return;
            }

            int loopIndex = Mathf.FloorToInt(loopsElapsed);
            float progress = loopsElapsed - loopIndex;
            if (_loopMode == LoopMode.Yoyo && (loopIndex & 1) == 1) progress = 1f - progress;

            Evaluate(progress);
        }

        // A Yoyo ends back at 0 after an even number of loops
        private float EndProgress(int loops)
        {
            return _loopMode == LoopMode.Yoyo && (loops & 1) == 0 ? 0f : 1f;
        }

        private void Complete(float endProgress)
        {
            // The end state is kept (e.g. a scale-in stays scaled) until Stop() or the next Play()
            Evaluate(endProgress);
            IsPlaying = false;
            _onComplete.Invoke();
        }
    }
}

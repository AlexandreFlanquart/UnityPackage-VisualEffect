namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Common contract of every effect of the package, so they can all be driven the same way
    /// (code, UnityEvent, Timeline, Feel feedback...).
    /// </summary>
    public interface IVisualEffect
    {
        /// <summary>True while the effect is running (including its start delay).</summary>
        bool IsPlaying { get; }

        /// <summary>Duration of one loop, in seconds.</summary>
        float Duration { get; }

        /// <summary>Starts the effect from the beginning. Restarts it if it is already playing.</summary>
        void Play();

        /// <summary>Stops the effect and removes everything it applied to its target.</summary>
        void Stop();
    }
}

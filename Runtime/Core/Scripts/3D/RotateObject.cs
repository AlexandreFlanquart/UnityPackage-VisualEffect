using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [Tooltip("Degrees to rotate per second around each axis (X, Y, Z)")]
    public Vector3 degreesPerSecond = new Vector3(0, 30, 0);

    [Tooltip("Choose between local (Self) or global (World) rotation")]
    public Space space = Space.Self;

    [Tooltip("Use unscaled time (not affected by time scale, useful for menus or pause)")]
    public bool useUnscaledTime = false;

    [Tooltip("Add some noise to the rotation speed (0 = speed not change, 1 = speed can vary between 0 and 100% more)")]
    [Range(0f, 1f)] public float noise = 0f;

    [Tooltip("Randomize starting phase to prevent synchronized rotation between objects")]
    public bool randomStartPhase = true;

    // Internal time offset (randomized if enabled)
    float _phase;

    void Awake()
    {
        if (randomStartPhase)
            _phase = Random.value * 100f; // random start time to desync noise
    }

    void Update()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        Vector3 d = degreesPerSecond;

        if (noise > 0f)
        {
            float t = (useUnscaledTime ? Time.unscaledTime : Time.time) + _phase;
            d += new Vector3(
                (Mathf.PerlinNoise(t, 0f) - 0.5f) * 2f * d.x * noise,
                (Mathf.PerlinNoise(0f, t) - 0.5f) * 2f * d.y * noise,
                (Mathf.PerlinNoise(t, t) - 0.5f) * 2f * d.z * noise
            );
        }

        transform.Rotate(d * dt, space);
    }
}


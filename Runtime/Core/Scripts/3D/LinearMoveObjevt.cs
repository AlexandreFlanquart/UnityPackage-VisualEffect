using UnityEngine;

public class LinearMoveObjevt : MonoBehaviour
{
    [Tooltip("Direction of movement (default is up)")]
    public Vector3 axis = Vector3.up;

    [Tooltip("Maximum distance from the start position (meters)")]
    public float amplitude = 0.1f;

    [Tooltip("Oscillation frequency (cycles per second)")]
    public float frequency = 1.0f;

    [Tooltip("Move in local space (true) or world space (false)")]
    public bool localSpace = true;

    [Tooltip("Use unscaled time (not affected by time scale, useful for menus or pause)")]
    public bool useUnscaledTime = false;

    [Tooltip("Randomize starting phase to prevent synchronized movement between objects")]
    public bool randomStartPhase = true;

    // Stores the initial position of the object
    Vector3 _startPos;
    // Internal time offset (randomized if enabled)
    float _phase;

    void Awake()
    {
        // Save the starting position depending on the chosen space
        _startPos = localSpace ? transform.localPosition : transform.position;
        // Randomize phase to desynchronize movement if enabled
        if (randomStartPhase) _phase = Random.value * 100f;
    }

    void Update()
    {
        // Calculate time based on chosen time scale
        float t = (useUnscaledTime ? Time.unscaledTime : Time.time) + _phase;
        // Calculate offset using sine wave
        Vector3 offset = axis.normalized * (Mathf.Sin(t * Mathf.PI * 2f * frequency) * amplitude);
        // Apply movement in local or world space
        if (localSpace) transform.localPosition = _startPos + offset;
        else            transform.position     = _startPos + offset;
    }

    void OnDisable()
    {
        // Reset to start position to avoid drifting when disabled
        if (localSpace) transform.localPosition = _startPos;
        else            transform.position     = _startPos;
    }
}

using UnityEngine;

public class FloatObject : MonoBehaviour
{
    [Header("⤴ Float Settings")]
    public Vector3 floatAxis = Vector3.up;   // Direction of floating
    public float floatAmplitude = 0.1f;      // Peak offset (meters)
    public float floatFrequency = 0.8f;      // Cycles per second
    public bool useUnscaledTime = false;
    public bool localSpace = true;
    public bool randomStartPhase = true;
    [Range(0f, 1f)] public float noise = 0.25f;   // Optional variation of amplitude
    public float noiseSpeed = 0.25f;

    [Header("↻ Rotation Settings")]
    public bool enableRotation = true;
    public Vector3 degreesPerSecond = new Vector3(0, 30, 0);
    public Space rotationSpace = Space.Self;
    [Range(0f, 1f)] public float rotationJitter = 0f;

    Vector3 _startPos;
    float _phase;
    float _noiseSeed;

    void Awake()
    {
        _startPos = localSpace ? transform.localPosition : transform.position;
        if (randomStartPhase) _phase = Random.value * Mathf.PI * 2f;
        _noiseSeed = Random.value * 999f;
    }

    void Update()
    {
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // 🔄 Rotation
        if (enableRotation && degreesPerSecond.sqrMagnitude > 0f)
        {
            Vector3 d = degreesPerSecond;
            if (rotationJitter > 0f)
            {
                float nT = t + _noiseSeed;
                d += new Vector3(
                    (Mathf.PerlinNoise(nT, 0f) - 0.5f) * 2f * d.x * rotationJitter,
                    (Mathf.PerlinNoise(0f, nT) - 0.5f) * 2f * d.y * rotationJitter,
                    (Mathf.PerlinNoise(nT, nT) - 0.5f) * 2f * d.z * rotationJitter
                );
            }

            transform.Rotate(d * dt, rotationSpace);
        }

        // 🌀 Floating
        float perlin = (Mathf.PerlinNoise(_noiseSeed, t * noiseSpeed) - 0.5f) * 2f;
        float amp = floatAmplitude * (1f + perlin * noise);
        float sine = Mathf.Sin((t + _phase) * Mathf.PI * 2f * floatFrequency);
        Vector3 offset = floatAxis.normalized * (sine * amp);

        if (localSpace) transform.localPosition = _startPos + offset;
        else            transform.position     = _startPos + offset;
    }

    void OnDisable()
    {
        if (localSpace) transform.localPosition = _startPos;
        else            transform.position     = _startPos;
    }
}

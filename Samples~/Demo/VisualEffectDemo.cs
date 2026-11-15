using MyUnityPackage.VisualEffect;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Drop this component on an empty GameObject and press Play:
/// it spawns primitives, each animated by one effect of the package
/// (three idle effects in the front row, five feedback effects in the second row).
/// </summary>
public class VisualEffectDemo : MonoBehaviour
{
    [SerializeField] private float _spacing = 2f;

    private void Start()
    {
        Spawn(PrimitiveType.Sphere, -_spacing, "FloatObject").AddComponent<FloatObject>();

        LinearMoveObject move = Spawn(PrimitiveType.Cube, 0f, "LinearMoveObject").AddComponent<LinearMoveObject>();
        move.Axis = Vector3.right;
        move.Amplitude = 0.5f;

        RotateObject rotate = Spawn(PrimitiveType.Capsule, _spacing, "RotateObject").AddComponent<RotateObject>();
        rotate.DegreesPerSecond = new Vector3(0f, 90f, 45f);

        // Second row: PulseEffect pulses by itself, the one-shots are looped to stay visible
        float z = _spacing;
        float x = -2f * _spacing;

        Spawn(PrimitiveType.Sphere, x, z, "PulseEffect").AddComponent<PulseEffect>();

        PunchEffect punch = Spawn(PrimitiveType.Cube, x + _spacing, z, "PunchEffect").AddComponent<PunchEffect>();
        Loop(punch);

        ShakeEffect shake = Spawn(PrimitiveType.Cube, x + 2f * _spacing, z, "ShakeEffect").AddComponent<ShakeEffect>();
        Loop(shake);

        SquashStretchEffect squash = Spawn(PrimitiveType.Sphere, x + 3f * _spacing, z, "SquashStretchEffect").AddComponent<SquashStretchEffect>();
        Loop(squash);

        ScaleEffect scale = Spawn(PrimitiveType.Capsule, x + 4f * _spacing, z, "ScaleEffect").AddComponent<ScaleEffect>();
        scale.Duration = 0.6f;
        Loop(scale);
    }

    // AddComponent runs OnEnable before the effect is configured, so Play() is called explicitly afterwards
    private static void Loop(VisualEffectBehaviour effect)
    {
        effect.Loops = -1;
        effect.PlayOnEnable = true;
        effect.Play();
    }

    private GameObject Spawn(PrimitiveType type, float x, string label)
    {
        return Spawn(type, x, 0f, label);
    }

    private GameObject Spawn(PrimitiveType type, float x, float z, string label)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = label;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(x, 0f, z);

        // Runtime primitives use the Built-in default material, which renders pink under URP/HDRP
        RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
        if (pipeline != null && pipeline.defaultMaterial != null)
        {
            go.GetComponent<Renderer>().sharedMaterial = pipeline.defaultMaterial;
        }

        return go;
    }
}

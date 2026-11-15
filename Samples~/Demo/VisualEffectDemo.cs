using MyUnityPackage.VisualEffect;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Drop this component on an empty GameObject and press Play:
/// it spawns three primitives, each animated by one effect of the package.
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
    }

    private GameObject Spawn(PrimitiveType type, float x, string label)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = label;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(x, 0f, 0f);

        // Runtime primitives use the Built-in default material, which renders pink under URP/HDRP
        RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
        if (pipeline != null && pipeline.defaultMaterial != null)
        {
            go.GetComponent<Renderer>().sharedMaterial = pipeline.defaultMaterial;
        }

        return go;
    }
}

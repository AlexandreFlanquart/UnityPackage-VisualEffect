using MyUnityPackage.VisualEffect;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// Builds a complete demo at runtime. In a new scene (camera + light), add this component to an empty
/// GameObject and press Play.
///
/// Row 1 (front)  : idle effects      - FloatObject, LinearMoveObject, RotateObject
/// Row 2 (middle) : transform effects - Pulse, Punch, Shake, SquashStretch, Scale
/// Row 3 (back)   : shader effects    - HitFlash and Dissolve on sprites (Sprite Effects shader)
/// Screen overlay : UI effects        - UIButtonFeedback with a punch on click, CanvasGroupFadeEffect
///
/// Not shown, because they need an optional asset: CameraImpulseEffect (Cinemachine),
/// MMF_VisualEffect (Feel), UIFaderAnimator (DOTween).
/// </summary>
public class VisualEffectDemo : MonoBehaviour
{
    private const string SpriteShaderName = "MyUnityPackage/Visual Effect/Sprite Effects";

    [Tooltip("Distance between two objects")]
    [SerializeField] private float _spacing = 2f;

    [Tooltip("Place the main camera so the whole demo is visible")]
    [SerializeField] private bool _frameCamera = true;

    private Font _font;

    private void Start()
    {
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        BuildIdleRow(0f);
        BuildTransformRow(_spacing);
        BuildShaderRow(2f * _spacing);
        BuildUI();

        if (_frameCamera) FrameCamera();
    }

    // ---------------------------------------------------------------- Row 1: idle effects

    private void BuildIdleRow(float z)
    {
        Spawn(PrimitiveType.Sphere, -_spacing, z, "FloatObject").AddComponent<FloatObject>();

        LinearMoveObject move = Spawn(PrimitiveType.Cube, 0f, z, "LinearMoveObject").AddComponent<LinearMoveObject>();
        move.Axis = Vector3.right;
        move.Amplitude = 0.5f;

        RotateObject rotate = Spawn(PrimitiveType.Capsule, _spacing, z, "RotateObject").AddComponent<RotateObject>();
        rotate.DegreesPerSecond = new Vector3(0f, 90f, 45f);
    }

    // ---------------------------------------------------------------- Row 2: transform effects

    private void BuildTransformRow(float z)
    {
        float x = -2f * _spacing;

        // PulseEffect loops and starts by itself
        Spawn(PrimitiveType.Sphere, x, z, "PulseEffect").AddComponent<PulseEffect>();

        // The others are one-shots: looped here so they stay visible
        Loop(Spawn(PrimitiveType.Cube, x + _spacing, z, "PunchEffect").AddComponent<PunchEffect>(), 1f);
        Loop(Spawn(PrimitiveType.Cube, x + 2f * _spacing, z, "ShakeEffect").AddComponent<ShakeEffect>(), 1f);
        Loop(Spawn(PrimitiveType.Sphere, x + 3f * _spacing, z, "SquashStretchEffect").AddComponent<SquashStretchEffect>(), 1f);
        Loop(Spawn(PrimitiveType.Capsule, x + 4f * _spacing, z, "ScaleEffect").AddComponent<ScaleEffect>(), 1f);
    }

    // ---------------------------------------------------------------- Row 3: shader effects

    private void BuildShaderRow(float z)
    {
        Shader shader = Shader.Find(SpriteShaderName);
        if (shader == null)
        {
            Debug.LogWarning($"VisualEffectDemo: shader '{SpriteShaderName}' not found, the shader row is skipped. Is URP installed and active?", this);
            return;
        }

        Sprite sprite = CreateDiscSprite();

        // One flash per second: slow enough to stay comfortable to watch
        HitFlashEffect flash = SpawnSprite(sprite, shader, -0.5f * _spacing, z, "HitFlashEffect", new Color(0.3f, 0.6f, 1f))
            .AddComponent<HitFlashEffect>();
        Loop(flash, 1f);

        DissolveEffect dissolve = SpawnSprite(sprite, shader, 0.5f * _spacing, z, "DissolveEffect", new Color(1f, 0.5f, 0.3f))
            .AddComponent<DissolveEffect>();
        dissolve.LoopType = VisualEffectBehaviour.LoopMode.Yoyo; // dissolve, then come back
        Loop(dissolve, 1.5f);
    }

    private GameObject SpawnSprite(Sprite sprite, Shader shader, float x, float z, string label, Color color)
    {
        var go = new GameObject(label);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(x, 0f, z);

        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.sharedMaterial = new Material(shader);

        AddLabel(go.transform, label);
        return go;
    }

    // A white disc, so the demo needs no texture asset
    private static Sprite CreateDiscSprite()
    {
        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius));
                byte alpha = distance <= radius - 1f ? (byte)255 : (byte)0;
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    // ---------------------------------------------------------------- Screen overlay: UI effects

    private void BuildUI()
    {
        var canvasObject = new GameObject("Demo Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        EnsureEventSystem();

        // Button: scales on hover / press (UIButtonFeedback) and punches when clicked
        RectTransform button = CreatePanel(canvasObject.transform, "Button (UIButtonFeedback + PunchEffect)",
            new Vector2(-260f, 120f), new Color(0.2f, 0.5f, 0.9f));
        button.gameObject.AddComponent<Button>();

        PunchEffect clickPunch = button.gameObject.AddComponent<PunchEffect>();
        clickPunch.Channel = TransformChannel.Rotation;
        clickPunch.Strength = new Vector3(0f, 0f, 8f);
        clickPunch.UseUnscaledTime = true;

        button.gameObject.AddComponent<UIButtonFeedback>().ClickEffect = clickPunch;

        // Panel: fades in and out forever
        RectTransform panel = CreatePanel(canvasObject.transform, "CanvasGroupFadeEffect",
            new Vector2(260f, 120f), new Color(0.9f, 0.4f, 0.2f));
        panel.gameObject.AddComponent<CanvasGroup>();

        CanvasGroupFadeEffect fade = panel.gameObject.AddComponent<CanvasGroupFadeEffect>();
        fade.LoopType = VisualEffectBehaviour.LoopMode.Yoyo;
        Loop(fade, 1f);
    }

    // Anchored to the bottom centre of the screen
    private RectTransform CreatePanel(Transform parent, string label, Vector2 position, Color color)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(460f, 110f);
        rect.anchoredPosition = position;
        go.GetComponent<Image>().color = color;

        var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(go.transform, false);

        var textRect = (RectTransform)textObject.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;

        Text text = textObject.GetComponent<Text>();
        text.text = label;
        text.font = _font;
        text.fontSize = 26;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false; // the Image receives the pointer events

        return rect;
    }

    // A Canvas needs an EventSystem to receive clicks. Only the legacy input module can be added from here:
    // the Input System one lives in an optional package this sample must not depend on.
    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

#if ENABLE_LEGACY_INPUT_MANAGER
        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystem.transform.SetParent(transform, false);
#else
        Debug.LogWarning("VisualEffectDemo: no EventSystem in the scene, the demo button will not react. Add one with GameObject > UI > Event System.", this);
#endif
    }

    // ---------------------------------------------------------------- Helpers

    // AddComponent runs OnEnable before the effect is configured, so Play() is called explicitly afterwards
    private static void Loop(VisualEffectBehaviour effect, float duration)
    {
        effect.Duration = duration;
        effect.Loops = -1;
        effect.Play();
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

        AddLabel(go.transform, label);
        return go;
    }

    // Name of the effect under each object. Kept as a sibling so the effect does not animate its own label.
    private void AddLabel(Transform target, string label)
    {
        var go = new GameObject(label + " Label");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = target.localPosition + new Vector3(0f, -1.2f, 0f);
        go.transform.localScale = Vector3.one * 0.08f;

        TextMesh text = go.AddComponent<TextMesh>();
        text.text = label;
        text.font = _font;
        text.fontSize = 48;
        text.anchor = TextAnchor.MiddleCenter;
        text.color = Color.white;
        go.GetComponent<MeshRenderer>().sharedMaterial = _font.material;
    }

    private void FrameCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 center = transform.TransformPoint(new Vector3(0f, 0f, _spacing));
        mainCamera.transform.position = center + transform.TransformVector(new Vector3(0f, 5f, -5.5f * _spacing));
        mainCamera.transform.LookAt(center);
    }
}

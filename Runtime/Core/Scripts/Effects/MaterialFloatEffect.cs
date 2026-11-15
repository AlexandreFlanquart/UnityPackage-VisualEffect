using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Animates a float property of a shader (flash amount, dissolve amount, outline width...) on one or
    /// several renderers. Works with any shader exposing the property, hand-written or made in Shader Graph.
    /// </summary>
    /// <remarks>
    /// Values are written through a MaterialPropertyBlock: the material asset is never modified and no
    /// material instance is created. The trade-off is that a renderer with a property block is not batched
    /// by the SRP Batcher while the effect holds it; the block is removed when the effect is reset.
    /// </remarks>
    [AddComponentMenu("MyUnityPackage/Visual Effect/Material Float Effect")]
    public class MaterialFloatEffect : VisualEffectBehaviour
    {
        [Header("Material")]
        [Tooltip("Renderers to animate (defaults to every Renderer on this object and its children)")]
        [SerializeField] private Renderer[] _renderers;

        [Tooltip("Reference name of the shader property (e.g. _FlashAmount), not its display name")]
        [SerializeField] private string _propertyName = "_FlashAmount";

        [SerializeField] private float _from = 1f;
        [SerializeField] private float _to;
        [SerializeField] private EaseSetting _ease = new EaseSetting(Ease.OutQuad);

        private MaterialPropertyBlock _block;
        private Renderer[] _resolved;

        // Per renderer: did it already have a property block before this effect touched it?
        private bool[] _hadBlock;
        private bool _applied;

        private int _propertyId;
        private string _propertyIdName;

        public string PropertyName { get => _propertyName; set => _propertyName = value; }
        public float From { get => _from; set => _from = value; }
        public float To { get => _to; set => _to = value; }

        /// <summary>Sets the renderers to animate. Stops the effect first so the previous ones are restored.</summary>
        public void SetRenderers(params Renderer[] renderers)
        {
            Stop();
            _renderers = renderers;
            _resolved = null;
        }

        /// <summary>Lets a preset subclass change the default property and range from its constructor.</summary>
        protected void SetPropertyDefaults(string propertyName, float from, float to, Ease ease)
        {
            _propertyName = propertyName;
            _from = from;
            _to = to;
            _ease = new EaseSetting(ease);
        }

        protected override void OnPlay()
        {
            if (_resolved == null)
            {
                _resolved = _renderers != null && _renderers.Length > 0
                    ? _renderers
                    : GetComponentsInChildren<Renderer>(true);
                _hadBlock = new bool[_resolved.Length];
            }
        }

        protected override void Evaluate(float progress)
        {
            if (_resolved == null || string.IsNullOrEmpty(_propertyName)) return;

            float value = Mathf.LerpUnclamped(_from, _to, _ease.Evaluate(progress));
            Write(value, false);
        }

        protected override void ResetEffect()
        {
            if (!_applied) return;

            Write(0f, true);
            _applied = false;
        }

        private void Write(float value, bool restore)
        {
            // Shader.PropertyToID cannot be called from a constructor or field initializer: resolve it lazily
            if (_propertyIdName != _propertyName)
            {
                _propertyId = Shader.PropertyToID(_propertyName);
                _propertyIdName = _propertyName;
            }

            _block ??= new MaterialPropertyBlock();

            for (int i = 0; i < _resolved.Length; i++)
            {
                Renderer target = _resolved[i];
                if (target == null) continue;

                if (!_applied) _hadBlock[i] = target.HasPropertyBlock();

                if (restore)
                {
                    if (!_hadBlock[i])
                    {
                        // The block only existed for this effect: remove it so the renderer batches again
                        target.SetPropertyBlock(null);
                        continue;
                    }

                    Material material = target.sharedMaterial;
                    value = material != null && material.HasFloat(_propertyId) ? material.GetFloat(_propertyId) : 0f;
                }

                // Get then Set: keeps the other properties already stored in the renderer's block
                target.GetPropertyBlock(_block);
                _block.SetFloat(_propertyId, value);
                target.SetPropertyBlock(_block);
            }

            _applied = true;
        }
    }
}

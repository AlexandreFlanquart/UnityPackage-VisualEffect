using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Fades the alpha of a CanvasGroup. Not additive: it saves the group's state when it starts
    /// and restores it on Stop.
    /// </summary>
    [AddComponentMenu("MyUnityPackage/Visual Effect/Canvas Group Fade Effect")]
    public class CanvasGroupFadeEffect : VisualEffectBehaviour
    {
        [Header("Fade")]
        [Tooltip("CanvasGroup to fade (defaults to the one on this object)")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Range(0f, 1f)]
        [SerializeField] private float _from = 0f;

        [Range(0f, 1f)]
        [SerializeField] private float _to = 1f;

        [SerializeField] private EaseSetting _ease = new EaseSetting(Ease.OutQuad);

        [Tooltip("Only let the group receive input once it is (almost) fully visible")]
        [SerializeField] private bool _controlInteractivity = true;

        // Rest state, captured when the effect starts
        private float _restAlpha;
        private bool _restInteractable;
        private bool _restBlocksRaycasts;
        private bool _hasRest;

        private CanvasGroup _cached;
        private bool _warned;

        public CanvasGroup CanvasGroup { get => _canvasGroup; set { _canvasGroup = value; _cached = null; } }
        public float From { get => _from; set => _from = Mathf.Clamp01(value); }
        public float To { get => _to; set => _to = Mathf.Clamp01(value); }

        private CanvasGroup Group
        {
            get
            {
                if (_canvasGroup != null) return _canvasGroup;
                if (_cached == null) _cached = GetComponent<CanvasGroup>();

                if (_cached == null && !_warned)
                {
                    _warned = true;
                    Debug.LogWarning("CanvasGroupFadeEffect: no CanvasGroup assigned or found on this object.", this);
                }

                return _cached;
            }
        }

        protected override void OnPlay()
        {
            CanvasGroup group = Group;
            if (group == null) return;

            _restAlpha = group.alpha;
            _restInteractable = group.interactable;
            _restBlocksRaycasts = group.blocksRaycasts;
            _hasRest = true;
        }

        protected override void Evaluate(float progress)
        {
            CanvasGroup group = Group;
            if (group == null) return;

            float alpha = Mathf.Clamp01(Mathf.LerpUnclamped(_from, _to, _ease.Evaluate(progress)));
            group.alpha = alpha;

            if (_controlInteractivity)
            {
                bool visible = alpha >= 0.99f;
                group.interactable = visible;
                group.blocksRaycasts = visible;
            }
        }

        protected override void ResetEffect()
        {
            if (!_hasRest) return;
            _hasRest = false;

            // Called by Play() before OnPlay() and possibly on a destroyed object: use the references already held
            CanvasGroup group = _canvasGroup != null ? _canvasGroup : _cached;
            if (group == null) return;

            group.alpha = _restAlpha;
            group.interactable = _restInteractable;
            group.blocksRaycasts = _restBlocksRaycasts;
        }
    }
}

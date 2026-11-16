using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Hover / pressed scale feedback for a button, plus an optional effect played on click.
    /// Works with or without a Selectable on the same object.
    /// </summary>
    [AddComponentMenu("MyUnityPackage/Visual Effect/UI Button Feedback")]
    [DisallowMultipleComponent]
    public class UIButtonFeedback : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Tooltip("Transform to scale (defaults to this object)")]
        [SerializeField] private Transform _target;

        [Tooltip("Scale multiplier while hovered")]
        [SerializeField] private float _hoverScale = 1.05f;

        [Tooltip("Scale multiplier while pressed")]
        [SerializeField] private float _pressedScale = 0.92f;

        [Tooltip("Smoothing time of the scale change (seconds)")]
        [Min(0.001f)]
        [SerializeField] private float _smoothTime = 0.08f;

        [Tooltip("Effect played when the button is clicked (optional)")]
        [SerializeField] private VisualEffectBehaviour _clickEffect;

        private TransformOffset _offset;
        private Selectable _selectable;

        private bool _hovered;
        private bool _pressed;
        private float _current = 1f;
        private float _velocity;

        public Transform Target { get => _target != null ? _target : transform; set => _target = value; }
        public VisualEffectBehaviour ClickEffect { get => _clickEffect; set => _clickEffect = value; }

        private bool CanInteract => _selectable == null || _selectable.IsInteractable();

        private float TargetScale
        {
            get
            {
                if (!CanInteract) return 1f;
                if (_pressed) return _pressedScale;
                return _hovered ? _hoverScale : 1f;
            }
        }

        private void Awake()
        {
            TryGetComponent(out _selectable);
        }

        private void OnDisable()
        {
            _hovered = false;
            _pressed = false;
            _current = 1f;
            _velocity = 0f;
            _offset.Clear(Target);
        }

        private void Update()
        {
            float targetScale = TargetScale;

            // Nothing to do while at rest: avoids touching the Transform every frame
            if (Mathf.Approximately(_current, targetScale) && Mathf.Abs(_velocity) < 0.0001f) return;

            // Unscaled time so menus keep animating while the game is paused
            _current = Mathf.SmoothDamp(_current, targetScale, ref _velocity, _smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

            if (Mathf.Abs(_current - targetScale) < 0.0005f && Mathf.Abs(_velocity) < 0.001f)
            {
                _current = targetScale;
                _velocity = 0f;
            }

            _offset.Apply(Target, TransformChannel.Scale, Vector3.one * (_current - 1f));
        }

        public void OnPointerEnter(PointerEventData eventData) { _hovered = true; }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            _pressed = false;
        }

        public void OnPointerDown(PointerEventData eventData) { _pressed = true; }

        public void OnPointerUp(PointerEventData eventData) { _pressed = false; }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_clickEffect != null && CanInteract) _clickEffect.Play();
        }
    }
}

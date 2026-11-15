using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Scales between two multipliers of the rest scale. Typical uses: pop in, pop out.
    /// </summary>
    [AddComponentMenu("MyUnityPackage/Visual Effect/Scale Effect")]
    public class ScaleEffect : VisualEffectBehaviour
    {
        [Header("Scale")]
        [Tooltip("Transform to animate (defaults to this object)")]
        [SerializeField] private Transform _target;

        [Tooltip("Scale multiplier at the start (1 = rest scale)")]
        [SerializeField] private Vector3 _from = Vector3.zero;

        [Tooltip("Scale multiplier at the end (1 = rest scale)")]
        [SerializeField] private Vector3 _to = Vector3.one;

        [SerializeField] private EaseSetting _ease = new EaseSetting(Ease.OutBack);

        private TransformOffset _offset;

        public Transform Target { get => _target != null ? _target : transform; set => _target = value; }
        public Vector3 From { get => _from; set => _from = value; }
        public Vector3 To { get => _to; set => _to = value; }

        protected override void Evaluate(float progress)
        {
            // Unclamped so overshooting eases (OutBack, OutElastic) go past the target
            Vector3 factor = Vector3.LerpUnclamped(_from, _to, _ease.Evaluate(progress));

            // The Scale channel expects a fraction of the rest scale, i.e. the multiplier minus one
            _offset.Apply(Target, TransformChannel.Scale, factor - Vector3.one);
        }

        protected override void ResetEffect()
        {
            _offset.Clear(Target);
        }
    }
}

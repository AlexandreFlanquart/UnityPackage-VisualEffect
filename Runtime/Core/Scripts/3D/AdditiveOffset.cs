using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Tracks the positional offset an effect has applied to a Transform, so the effect only ever
    /// adds/removes its own contribution instead of overwriting the position.
    /// This lets several effects (or gameplay code) move the same Transform without fighting.
    /// </summary>
    internal struct AdditiveOffset
    {
        private Vector3 _applied;
        private bool _appliedInLocalSpace;

        /// <summary>Replaces the previously applied offset with <paramref name="offset"/>.</summary>
        public void Apply(Transform target, Vector3 offset, bool localSpace)
        {
            // Space changed since last frame: remove the old contribution in the space it was applied in
            if (localSpace != _appliedInLocalSpace) Clear(target);

            Vector3 delta = offset - _applied;
            if (localSpace) target.localPosition += delta;
            else            target.position += delta;

            _applied = offset;
            _appliedInLocalSpace = localSpace;
        }

        /// <summary>Removes the applied offset, restoring the position the Transform would have without the effect.</summary>
        public void Clear(Transform target)
        {
            if (_applied != Vector3.zero && target != null)
            {
                if (_appliedInLocalSpace) target.localPosition -= _applied;
                else                      target.position -= _applied;
            }

            _applied = Vector3.zero;
        }
    }
}

using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>Which part of a Transform an effect animates.</summary>
    public enum TransformChannel
    {
        /// <summary>Local position offset, in units.</summary>
        Position,
        /// <summary>Local rotation offset, in degrees (Euler).</summary>
        Rotation,
        /// <summary>Scale offset as a fraction of the rest scale (0.2 = 20% bigger).</summary>
        Scale
    }

    /// <summary>
    /// Tracks what an effect has applied to a Transform (position, rotation and scale), so the effect only
    /// ever adds or removes its own contribution. Several effects can then animate the same Transform,
    /// and gameplay code can keep moving it, without anyone overwriting anyone else.
    /// </summary>
    internal struct TransformOffset
    {
        private AdditiveOffset _position;

        private Quaternion _rotation;
        private bool _hasRotation;

        private Vector3 _scale;
        private Vector3 _restScale;
        private bool _hasScale;

        /// <summary>Replaces the offset previously applied on <paramref name="channel"/> by <paramref name="value"/>.</summary>
        public void Apply(Transform target, TransformChannel channel, Vector3 value)
        {
            switch (channel)
            {
                case TransformChannel.Position:
                    _position.Apply(target, value, true);
                    break;

                case TransformChannel.Rotation:
                    Quaternion rotation = Quaternion.Euler(value);
                    Quaternion previous = _hasRotation ? _rotation : Quaternion.identity;
                    // localRotation = rest * previous, so rest * rotation = localRotation * previous^-1 * rotation
                    target.localRotation = target.localRotation * Quaternion.Inverse(previous) * rotation;
                    _rotation = rotation;
                    _hasRotation = true;
                    break;

                case TransformChannel.Scale:
                    if (!_hasScale)
                    {
                        _restScale = target.localScale;
                        _hasScale = true;
                    }

                    // Stored as an additive offset rather than a multiplier, so scaling down to zero can be undone
                    Vector3 offset = Vector3.Scale(_restScale, value);
                    target.localScale += offset - _scale;
                    _scale = offset;
                    break;
            }
        }

        /// <summary>Removes everything applied, on every channel.</summary>
        public void Clear(Transform target)
        {
            _position.Clear(target);

            if (target != null)
            {
                if (_hasRotation) target.localRotation = target.localRotation * Quaternion.Inverse(_rotation);
                if (_hasScale) target.localScale -= _scale;
            }

            _rotation = Quaternion.identity;
            _hasRotation = false;
            _scale = Vector3.zero;
            _hasScale = false;
        }
    }
}

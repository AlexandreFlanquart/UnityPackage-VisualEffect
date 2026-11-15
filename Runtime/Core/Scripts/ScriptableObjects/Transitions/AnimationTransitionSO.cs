using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Base class for all animation transitions.
    /// Plays <see cref="animationClip"/> on the target through an AnimatorOverrideController built from
    /// <see cref="animatorController"/> (its first clip is replaced by the transition clip).
    /// </summary>
    public abstract class AnimationTransitionSO : TransitionSO
    {
        [SerializeField] protected AnimationClip _animationClip;
        [SerializeField] protected RuntimeAnimatorController _animatorController;

        [Tooltip("Name of the state to play in the Animator Controller (layer 0)")]
        [SerializeField] protected string _stateName = "Default";

        public AnimationClip animationClip => _animationClip;
        public RuntimeAnimatorController animatorController => _animatorController;

        // Built once and shared by every target: controllers are assets, not per-instance state
        private AnimatorOverrideController _overrideController;

        public override async Task PlayTransition(GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning($"[{name}] Transition target is null.", this);
                return;
            }

            AnimatorOverrideController controller = GetOverrideController();
            if (controller == null) return;

            if (!target.TryGetComponent(out Animator animator))
            {
                animator = target.AddComponent<Animator>();
            }

            animator.runtimeAnimatorController = controller;

            int stateHash = Animator.StringToHash(_stateName);
            if (!animator.HasState(0, stateHash))
            {
                Debug.LogWarning($"[{name}] State '{_stateName}' not found on layer 0 of '{_animatorController.name}'.", this);
                return;
            }

            animator.Play(stateHash, 0, 0f);

            // Wait for the clip to finish, on the same clock as the Animator
            bool unscaled = animator.updateMode == AnimatorUpdateMode.UnscaledTime;
            await RunOverTime(target, _animationClip.length, unscaled);
        }

        private AnimatorOverrideController GetOverrideController()
        {
            if (_overrideController != null) return _overrideController;

            if (_animatorController == null || _animationClip == null)
            {
                Debug.LogWarning($"[{name}] Animator Controller or Animation Clip is not assigned.", this);
                return null;
            }

            var overrideController = new AnimatorOverrideController(_animatorController)
            {
                name = $"{name} (Override)",
                hideFlags = HideFlags.HideAndDontSave
            };

            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);

            if (overrides.Count == 0)
            {
                Debug.LogWarning($"[{name}] '{_animatorController.name}' contains no clip to override.", this);
                DestroyController(overrideController);
                return null;
            }

            // Replace the first clip of the controller by the transition clip
            overrides[0] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[0].Key, _animationClip);
            overrideController.ApplyOverrides(overrides);

            _overrideController = overrideController;
            return _overrideController;
        }

        // Rebuild the cached controller when the asset is edited, release it when the asset is unloaded
        private void OnValidate() => ReleaseOverrideController();
        private void OnDisable() => ReleaseOverrideController();

        private void ReleaseOverrideController()
        {
            if (_overrideController == null) return;

            DestroyController(_overrideController);
            _overrideController = null;
        }

        private static void DestroyController(Object controller)
        {
            if (Application.isPlaying) Destroy(controller);
            else DestroyImmediate(controller);
        }
    }
}

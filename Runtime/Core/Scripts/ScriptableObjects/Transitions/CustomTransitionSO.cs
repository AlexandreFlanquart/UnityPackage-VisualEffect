using System.Threading.Tasks;
using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Base class for all custom (by script) transitions.
    /// </summary>
    public abstract class CustomTransitionSO : TransitionSO
    {
        public override Task PlayTransition(GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning($"[{name}] Transition target is null.", this);
                return Task.CompletedTask;
            }

            // Subclasses implement their own transition logic
            return OnCustomTransition(target);
        }

        protected abstract Task OnCustomTransition(GameObject target);
    }
}

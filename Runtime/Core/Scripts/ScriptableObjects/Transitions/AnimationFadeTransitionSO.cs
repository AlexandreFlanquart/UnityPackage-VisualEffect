using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Fade transition played from an Animation Clip.
    /// Override <see cref="AnimationTransitionSO.PlayTransition"/> to add logic before or after the animation
    /// (the base call completes when the clip has finished playing).
    /// </summary>
    [CreateAssetMenu(fileName = "AnimationFadeTransitionSO", menuName = "MyUnityPackage/Visual Effect/Transitions/Animation Fade")]
    public class AnimationFadeTransitionSO : AnimationTransitionSO
    {
    }
}

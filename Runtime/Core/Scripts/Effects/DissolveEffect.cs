using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Dissolves the object (death, teleport, disappearing pickup). Swap From and To to make it appear.
    /// Preset of <see cref="MaterialFloatEffect"/> driving <c>_DissolveAmount</c> from 0 to 1.
    /// Requires a shader exposing that property, such as "MyUnityPackage/Visual Effect/Sprite Effects".
    /// </summary>
    [AddComponentMenu("MyUnityPackage/Visual Effect/Dissolve Effect")]
    public class DissolveEffect : MaterialFloatEffect
    {
        public DissolveEffect()
        {
            SetPlaybackDefaults(duration: 1f);
            SetPropertyDefaults("_DissolveAmount", 0f, 1f, Ease.Linear);
        }
    }
}

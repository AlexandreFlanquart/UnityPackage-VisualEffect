using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Flashes the object with the shader's flash colour, then fades back (damage feedback).
    /// Preset of <see cref="MaterialFloatEffect"/> driving <c>_FlashAmount</c> from 1 to 0.
    /// Requires a shader exposing that property, such as "MyUnityPackage/Visual Effect/Sprite Effects".
    /// </summary>
    [AddComponentMenu("MyUnityPackage/Visual Effect/Hit Flash Effect")]
    public class HitFlashEffect : MaterialFloatEffect
    {
        public HitFlashEffect()
        {
            SetPlaybackDefaults(duration: 0.15f);
            SetPropertyDefaults("_FlashAmount", 1f, 0f, Ease.OutQuad);
        }
    }
}

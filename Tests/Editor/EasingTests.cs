using System;
using NUnit.Framework;

namespace MyUnityPackage.VisualEffect.Tests
{
    public class EasingTests
    {
        private static Ease[] Presets()
        {
            var all = (Ease[])Enum.GetValues(typeof(Ease));
            return Array.FindAll(all, ease => ease != Ease.Custom);
        }

        [TestCaseSource(nameof(Presets))]
        public void Preset_StartsAtZero_EndsAtOne(Ease ease)
        {
            Assert.AreEqual(0f, Easing.Evaluate(ease, 0f), 1e-4f);
            Assert.AreEqual(1f, Easing.Evaluate(ease, 1f), 1e-4f);
        }

        [TestCaseSource(nameof(Presets))]
        public void Preset_ClampsInputOutsideRange(Ease ease)
        {
            Assert.AreEqual(Easing.Evaluate(ease, 0f), Easing.Evaluate(ease, -1f), 1e-4f);
            Assert.AreEqual(Easing.Evaluate(ease, 1f), Easing.Evaluate(ease, 2f), 1e-4f);
        }
    }
}

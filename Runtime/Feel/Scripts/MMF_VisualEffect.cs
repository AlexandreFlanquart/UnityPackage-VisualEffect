using MoreMountains.Feedbacks;
using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Feel feedback that plays or stops an effect of this package, so package effects (including the
    /// shader-driven ones) can be sequenced in an MMF_Player next to Feel's own feedbacks.
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("Plays or stops an effect of the MyUnityPackage Visual Effect package (Punch, Shake, Hit Flash, Dissolve...).")]
    [FeedbackPath("MyUnityPackage/Visual Effect")]
    public class MMF_VisualEffect : MMF_Feedback
    {
        /// <summary>Feel convention: lets every feedback of this type be disabled at once.</summary>
        public static bool FeedbackTypeAuthorized = true;

        public enum Modes { Play, Stop }

#if UNITY_EDITOR
        public override bool EvaluateRequiresSetup() { return TargetEffect == null; }
        public override string RequiredTargetText { get { return TargetEffect != null ? TargetEffect.name : ""; } }
        public override string RequiresSetupText { get { return "This feedback requires that a TargetEffect be set to be able to work properly."; } }
#endif

        [MMFInspectorGroup("Visual Effect", true, 12, true)]
        [Tooltip("The effect to play or stop")]
        public VisualEffectBehaviour TargetEffect;

        [Tooltip("What this feedback does to the effect when it plays")]
        public Modes Mode = Modes.Play;

        /// <summary>Lets the MMF_Player know how long this feedback lasts (0 for Stop or an endless effect).</summary>
        public override float FeedbackDuration
        {
            get
            {
                if (TargetEffect == null || Mode == Modes.Stop) return 0f;

                float total = TargetEffect.TotalDuration;
                return float.IsInfinity(total) ? 0f : total;
            }
            set { }
        }

        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized || TargetEffect == null) return;

            if (Mode == Modes.Play) TargetEffect.Play();
            else TargetEffect.Stop();
        }

        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized || TargetEffect == null) return;

            TargetEffect.Stop();
        }
    }
}

using NUnit.Framework;
using UnityEngine;

namespace MyUnityPackage.VisualEffect.Tests
{
    /// <summary>
    /// Drives effects with Tick() in Edit Mode: no player loop needed, and time is fully deterministic.
    /// </summary>
    public class EffectPlaybackTests
    {
        private const float Tolerance = 1e-3f;

        private GameObject _go;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("EffectTest");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            VisualEffectSettings.MotionScale = 1f;
            VisualEffectSettings.FlashScale = 1f;
        }

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.Less(Vector3.Distance(expected, actual), Tolerance, $"Expected {expected}, got {actual}");
        }

        [Test]
        public void Punch_AnimatesThenReturnsToRest()
        {
            _go.transform.localScale = new Vector3(2f, 2f, 2f);
            PunchEffect punch = _go.AddComponent<PunchEffect>();
            punch.Duration = 1f;

            punch.Play();
            punch.Tick(0.1f);
            Assert.Greater(Vector3.Distance(new Vector3(2f, 2f, 2f), _go.transform.localScale), Tolerance);
            Assert.IsTrue(punch.IsPlaying);

            punch.Tick(1f);
            Assert.IsFalse(punch.IsPlaying);
            AssertVector(new Vector3(2f, 2f, 2f), _go.transform.localScale);
        }

        [Test]
        public void StackedEffects_RestoreRestScaleOnStop()
        {
            _go.transform.localScale = new Vector3(2f, 3f, 4f);
            PunchEffect punch = _go.AddComponent<PunchEffect>();
            PulseEffect pulse = _go.AddComponent<PulseEffect>();
            punch.Duration = 1f;

            punch.Play();
            pulse.Play();
            punch.Tick(0.1f);
            pulse.Tick(0.1f);

            // Stopped in the same order they started: each one only removes its own contribution
            punch.Stop();
            pulse.Stop();

            AssertVector(new Vector3(2f, 3f, 4f), _go.transform.localScale);
        }

        [Test]
        public void PositionEffect_KeepsGameplayMovement()
        {
            ShakeEffect shake = _go.AddComponent<ShakeEffect>();
            shake.Duration = 1f;

            shake.Play();
            shake.Tick(0.1f);
            _go.transform.localPosition += new Vector3(10f, 0f, 0f); // gameplay moves the object mid-effect
            shake.Tick(0.1f);
            shake.Stop();

            AssertVector(new Vector3(10f, 0f, 0f), _go.transform.localPosition);
        }

        [Test]
        public void Scale_HoldsEndState_UntilStop()
        {
            ScaleEffect scale = _go.AddComponent<ScaleEffect>();
            scale.Duration = 1f;
            scale.From = Vector3.one;
            scale.To = Vector3.zero;

            scale.Play();
            scale.Tick(2f);
            Assert.IsFalse(scale.IsPlaying);
            AssertVector(Vector3.zero, _go.transform.localScale);

            scale.Stop();
            AssertVector(Vector3.one, _go.transform.localScale);
        }

        [Test]
        public void Play_AppliesFirstFrameImmediately()
        {
            ScaleEffect scale = _go.AddComponent<ScaleEffect>(); // defaults: from 0 to 1

            scale.Play();

            AssertVector(Vector3.zero, _go.transform.localScale);
        }

        [Test]
        public void Delay_PostponesTheEffect()
        {
            ScaleEffect scale = _go.AddComponent<ScaleEffect>();
            scale.Duration = 1f;
            scale.Delay = 0.5f;

            scale.Play();
            scale.Tick(0.25f);
            AssertVector(Vector3.one, _go.transform.localScale);
            Assert.IsTrue(scale.IsPlaying);

            scale.Tick(0.3f);
            Assert.Greater(Vector3.Distance(Vector3.one, _go.transform.localScale), Tolerance);
        }

        [Test]
        public void Loops_CompleteOnce_AfterAllLoops()
        {
            PunchEffect punch = _go.AddComponent<PunchEffect>();
            punch.Duration = 1f;
            punch.Loops = 2;
            int completed = 0;
            punch.OnComplete.AddListener(() => completed++);

            punch.Play();
            punch.Tick(1.5f);
            Assert.IsTrue(punch.IsPlaying);
            Assert.AreEqual(0, completed);

            punch.Tick(0.6f);
            Assert.IsFalse(punch.IsPlaying);
            Assert.AreEqual(1, completed);

            punch.Tick(1f); // ticking a finished effect does nothing
            Assert.AreEqual(1, completed);
        }

        [Test]
        public void Yoyo_EvenLoops_EndsWhereItStarted()
        {
            ScaleEffect scale = _go.AddComponent<ScaleEffect>(); // from 0 to 1
            scale.Duration = 1f;
            scale.Loops = 2;
            scale.LoopType = VisualEffectBehaviour.LoopMode.Yoyo;

            scale.Play();
            scale.Tick(3f);

            AssertVector(Vector3.zero, _go.transform.localScale);
        }

        [Test]
        public void InfiniteLoops_NeverComplete()
        {
            PulseEffect pulse = _go.AddComponent<PulseEffect>(); // loops forever by default
            int completed = 0;
            pulse.OnComplete.AddListener(() => completed++);

            pulse.Play();
            pulse.Tick(100f);

            Assert.IsTrue(pulse.IsPlaying);
            Assert.AreEqual(0, completed);
            Assert.IsTrue(float.IsInfinity(pulse.TotalDuration));
        }

        [Test]
        public void MotionScaleZero_DisablesDecorativeMotion()
        {
            VisualEffectSettings.MotionScale = 0f;
            PunchEffect punch = _go.AddComponent<PunchEffect>();
            punch.Duration = 1f;

            punch.Play();
            punch.Tick(0.1f);

            AssertVector(Vector3.one, _go.transform.localScale);
        }

        [Test]
        public void MotionScaleZero_StillLetsFunctionalScaleFinish()
        {
            VisualEffectSettings.MotionScale = 0f;
            ScaleEffect scale = _go.AddComponent<ScaleEffect>();
            scale.Duration = 1f;
            scale.From = Vector3.one;
            scale.To = Vector3.zero;

            scale.Play();
            scale.Tick(2f);

            AssertVector(Vector3.zero, _go.transform.localScale);
        }
    }
}

using DG.Tweening;
using UnityEngine;

namespace MyUnityPackage.VisualEffect
{
    /// <summary>
    /// Responsabilité unique : gère l'animation de fade (transparence) et l'interactivité d'un CanvasGroup.
    /// </summary>
    [AddComponentMenu("MyUnityPackage/Visual Effect/UI Fader Animator (DOTween)")]
    [RequireComponent(typeof(CanvasGroup))]
    public class UIFaderAnimator : MonoBehaviour
    {
        // Valeurs explicites : l'enum est sérialisée par valeur, 1 et 2 étaient d'anciens modes sans effet visible.
        public enum LoopMode
        {
            None = 0,
            BothInOut = 3
        }

        [Header("Fade Settings")]
        public float duration = 0.25f;
        public Ease easeIn = Ease.OutQuad;
        public Ease easeOut = Ease.Linear;
        [Tooltip("Ignore Time.timeScale : le fade continue quand le jeu est en pause.")]
        public bool useUnscaledTime = true;

        [Header("Loop Settings")]
        [Tooltip("None : simple fade in. BothInOut : clignotement (apparition puis disparition).")]
        public LoopMode loopType = LoopMode.None;
        [Tooltip("Nombre de boucles (-1 pour infini).")]
        public int loopCount = -1;
        [Tooltip("Type de boucle DOTween.")]
        public LoopType dotweenLoopType = LoopType.Yoyo;

        [Header("Options de Démarrage Indépendant")]
        [Tooltip("L'objet commence invisible et joue son animation à chaque activation.")]
        public bool startOnAwake = false;
        public float startDelay = 0.1f;

        private CanvasGroup canvasGroup;
        private Tween currentTween;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            if (!startOnAwake)
            {
                canvasGroup.alpha = 1f;
                SetInteractable(true);
            }
        }

        private void OnEnable()
        {
            if (!startOnAwake) return;

            canvasGroup.alpha = 0f;
            SetInteractable(false);
            StartLooping(startDelay);
        }

        /// <summary>
        /// Démarre un fade in simple ou un clignotement selon le LoopMode sélectionné.
        /// </summary>
        public void StartLooping(float delay = 0f)
        {
            if (loopType != LoopMode.BothInOut)
            {
                FadeIn(delay);
                return;
            }

            KillCurrentTween();

            // Clignotement : l'objet part invisible et n'est jamais interactif
            canvasGroup.alpha = 0f;
            SetInteractable(false);

            Sequence loopSequence = DOTween.Sequence()
                .Append(canvasGroup.DOFade(1f, duration).SetEase(easeIn))
                .Append(canvasGroup.DOFade(0f, duration).SetEase(easeOut))
                .SetDelay(delay)
                .SetLoops(loopCount, dotweenLoopType);

            currentTween = Configure(loopSequence);
        }

        public Tween FadeIn(float delay = 0f)
        {
            KillCurrentTween();
            SetInteractable(false);

            currentTween = Configure(canvasGroup.DOFade(1f, duration)
                .SetEase(easeIn)
                .SetDelay(delay)
                .OnComplete(() => SetInteractable(true)));

            return currentTween;
        }

        public Tween FadeOut(float delay = 0f)
        {
            KillCurrentTween();
            SetInteractable(false);

            currentTween = Configure(canvasGroup.DOFade(0f, duration)
                .SetEase(easeOut)
                .SetDelay(delay));

            return currentTween;
        }

        // Réglages communs : horloge, et tween détruit avec le GameObject
        private Tween Configure(Tween tween)
        {
            return tween
                .SetUpdate(useUnscaledTime)
                .SetLink(gameObject);
        }

        // Tue le tween sans le compléter : pas de saut d'alpha ni de OnComplete parasite
        private void KillCurrentTween()
        {
            if (currentTween != null && currentTween.IsActive())
            {
                currentTween.Kill(false);
            }

            currentTween = null;
        }

        private void SetInteractable(bool isInteractable)
        {
            canvasGroup.interactable = isInteractable;
            canvasGroup.blocksRaycasts = isInteractable;
        }

        private void OnDisable()
        {
            KillCurrentTween();
        }
    }
}

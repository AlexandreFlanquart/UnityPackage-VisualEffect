using UnityEngine;
using DG.Tweening;

/// <summary>
/// Responsabilité Unique : Gère l'animation de Fade (transparence) et l'interactivité.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIFaderAnimator : MonoBehaviour
{
    // Nouvelle énumération pour définir le type de boucle (uniquement In, Out, ou les deux)
    public enum LoopMode { None, FadeIn, FadeOut, BothInOut }

    [Header("Fade Settings")]
    public float duration = 0.25f;
    public Ease easeIn = Ease.OutQuad; // J'ai mis OutQuad par défaut pour les alertes rapides
    public Ease easeOut = Ease.Linear;
    
    [Header("Loop Settings")]
    [Tooltip("Définit si et comment l'animation doit boucler.")]
    public LoopMode loopType = LoopMode.None;
    [Tooltip("Nombre de boucles (-1 pour infini).")]
    public int loopCount = -1;
    [Tooltip("Type de boucle DOTween (Yoyo est souvent le meilleur pour les boucles).")]
    public LoopType dotweenLoopType = LoopType.Yoyo; 

    [Header("Options de Démarrage Indépendant")]
    [Tooltip("L'objet commence invisible et Fade In au démarrage.")]
    public bool startOnAwake = false; 
    public float startDelay = 0.1f;

    private CanvasGroup canvasGroup;
    private Tween currentTween;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Initialisation de l'état
        canvasGroup.alpha = startOnAwake ? 0f : 1f; 
        SetInteractable(!startOnAwake); 

        // Gestion du démarrage
        if (startOnAwake)
        {
            if (loopType == LoopMode.None)
            {
                FadeIn(startDelay);
            }
            else
            {
                StartLooping(startDelay);
            }
        }
    }

    /// <summary>
    /// Démarre une animation ou une boucle complète selon le LoopMode sélectionné.
    /// </summary>
    public void StartLooping(float delay = 0f)
    {
        // Si aucune boucle n'est définie, on fait un simple Fade In
        if (loopType == LoopMode.None)
        {
            FadeIn(delay);
            return;
        }

        // 1. On s'assure d'arrêter tout ce qui est en cours
        currentTween.Kill(true);

        // 2. Création de la séquence de boucle
        Sequence loopSequence = DOTween.Sequence()
            .SetDelay(delay);

        // 3. Définition des étapes dans la boucle
        if (loopType == LoopMode.FadeIn)
        {
            // Boucle : Opaque -> Opaque (utile si on veut juste répéter le FadeIn pour rien)
            loopSequence.Append(canvasGroup.DOFade(1f, duration).SetEase(easeIn));
        }
        else if (loopType == LoopMode.FadeOut)
        {
            // Boucle : Transparent -> Transparent 
            loopSequence.Append(canvasGroup.DOFade(0f, duration).SetEase(easeOut));
        }
        else if (loopType == LoopMode.BothInOut)
        {
            // Boucle : Apparition -> Disparition (Clignotement)
            // L'objet doit commencer invisible pour que le FadeIn soit la première étape.
            canvasGroup.alpha = 0f; 
            SetInteractable(false); // Pas d'interactivité pendant le clignotement

            loopSequence
                .Append(canvasGroup.DOFade(1f, duration).SetEase(easeIn))
                .Append(canvasGroup.DOFade(0f, duration).SetEase(easeOut));
        }

        // 4. Application des paramètres de boucle et lancement
        currentTween = loopSequence
            .SetLoops(loopCount, dotweenLoopType)
            .SetLink(gameObject) // Optimisation : lie le tween à l'objet
            .OnComplete(() => {
                // Rendre interactif seulement si l'animation s'arrête à l'état opaque
                if (loopType == LoopMode.FadeIn || loopType == LoopMode.None)
                {
                    SetInteractable(true);
                }
            })
            .Play();
    }
    
    // --- Fonctions d'animation simples (utiles pour un appel direct sans boucle) ---

    public Tween FadeIn(float delay = 0f)
    {    
        currentTween.Kill(true);
        SetInteractable(false);
        
        return canvasGroup.DOFade(1f, duration) 
            .SetEase(easeIn) 
            .SetDelay(delay)
            .OnComplete(() => SetInteractable(true));
    }

    public Tween FadeOut(float delay = 0f)
    {    
        currentTween.Kill(true);
        SetInteractable(false);

        return canvasGroup.DOFade(0f, duration)
            .SetEase(easeOut) 
            .SetDelay(delay);
    }
    
    // --- Fonctions Utilitaires ---
    
    // Fonction utilitaire pour gérer l'interactivité
    private void SetInteractable(bool isInteractable)
    {
        canvasGroup.interactable = isInteractable;
        canvasGroup.blocksRaycasts = isInteractable;
    }
    
    void OnDisable()
    {
        currentTween.Kill(false);
    }
}
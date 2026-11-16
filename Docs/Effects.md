## ✨ Système d'Effets

Tous les types sont dans le namespace `MyUnityPackage.VisualEffect`.

### Le contrat

#### [`IVisualEffect.cs`](../Runtime/Core/Scripts/Effects/IVisualEffect.cs)
Contrat commun de tous les effets : ils se pilotent tous de la même façon (code, UnityEvent, Timeline...).

```csharp
public interface IVisualEffect
{
    bool IsPlaying { get; }   // vrai pendant l'effet, délai inclus
    float Duration { get; }   // durée d'une boucle, en secondes
    void Play();              // (re)démarre depuis le début
    void Stop();              // arrête et retire tout ce que l'effet a appliqué
}
```

#### [`VisualEffectBehaviour.cs`](../Runtime/Core/Scripts/Effects/VisualEffectBehaviour.cs)
Classe de base : elle gère la lecture, les sous-classes décrivent seulement l'état de l'effet pour un `progress` donné (0 à 1).

| Champ | Rôle |
|-------|------|
| `PlayOnEnable` | Joue automatiquement à chaque activation du composant |
| `Duration` | Durée d'une boucle (secondes) |
| `Delay` | Attente avant le début (secondes) |
| `Loops` | Nombre de boucles (-1 = infini) |
| `LoopType` | `Restart` (0 vers 1 à chaque boucle) ou `Yoyo` (aller-retour) |
| `UseUnscaledTime` | Ignore `Time.timeScale` (menus, pause) |
| `OnComplete` | `UnityEvent` appelé à la fin (jamais en boucle infinie ni après `Stop`) |

### Les effets

| Effet | Usage |
|-------|-------|
| `PunchEffect` | Impact ponctuel qui oscille puis revient au repos (clic, coup reçu, objet ramassé) |
| `PulseEffect` | Oscillation continue (bouton qui respire, icône qui clignote) |
| `ScaleEffect` | Passage d'un multiplicateur d'échelle à un autre (apparition / disparition en "pop") |
| `ShakeEffect` | Secousse aléatoire qui s'estompe (dégâts, explosion) |
| `SquashStretchEffect` | Écrasement / étirement à volume constant (atterrissage, rebond) |
| `CanvasGroupFadeEffect` | Fondu de l'alpha d'un `CanvasGroup`, avec gestion de l'interactivité |
| `MaterialFloatEffect` | Anime une propriété float de shader (par son Reference name) sur un ou plusieurs `Renderer` |
| `HitFlashEffect` | Preset de `MaterialFloatEffect` : flash de dégât (`_FlashAmount` de 1 à 0) |
| `DissolveEffect` | Preset de `MaterialFloatEffect` : dissolution (`_DissolveAmount` de 0 à 1) |

Les trois effets matériau nécessitent un shader exposant la propriété visée : voir [Shaders.md](Shaders.md).

Les effets de transformation (`Position`, `Rotation`, `Scale`) ont une cible optionnelle (`Target`, par défaut l'objet lui-même). Pour la valeur `Scale`, les valeurs sont une **fraction** de l'échelle de repos : `0.2` = +20 %, `-1` = échelle nulle.

### Empilement des effets

Les effets sont **additifs** : chacun mémorise ce qu'il a appliqué et ne retire que sa propre contribution. On peut donc empiler plusieurs effets sur le même Transform, et continuer à déplacer l'objet depuis le gameplay, sans que personne n'écrase personne.

> **Attention** : un effet de `Position` sur un `RectTransform` piloté par un `LayoutGroup` doit être placé sur un **objet enfant**, sinon le layout recalcule la position et entre en conflit avec l'effet.

`CanvasGroupFadeEffect` n'est pas additif : il sauvegarde l'état du `CanvasGroup` au démarrage et le restaure au `Stop`.

### Déclencher un effet

Depuis le code :

```csharp
[SerializeField] private PunchEffect _punch;

void OnHit()
{
    _punch.Play();
}
```

Depuis un `UnityEvent` (bouton, animation...) : choisir la méthode `Play` du composant dans l'Inspector.

Pour un effet ponctuel qui doit rester lisible en boucle (démo), configurer puis appeler `Play()` explicitement : `AddComponent` exécute `OnEnable` avant la configuration.

### Prévisualisation dans l'Inspector

Chaque effet affiche les boutons **Play** et **Stop**. Hors Play Mode, la prévisualisation tourne dans la Scene view et l'objet retrouve son état à la fin (ou au changement de sélection) : rien n'est modifié durablement.

### Module UGUI

`UIButtonFeedback` (assembly `MyUnityPackage.VisualEffect.UGUI`) donne un retour de survol et d'appui à un bouton (échelle lissée, temps non affecté par `timeScale`) et peut jouer un effet au clic (`_clickEffect`). Il ignore le survol et l'appui si le `Selectable` n'est pas interactable.

### Module Feel

Avec [Feel](https://feel.moremountains.com) installé, le feedback `MMF_VisualEffect` (**Add new feedback > MyUnityPackage > Visual Effect**) joue ou arrête un effet du package depuis un `MMF_Player`. On peut ainsi enchaîner un `HitFlashEffect`, un son et un freeze frame de Feel dans la même séquence. Le feedback annonce au player la durée totale de l'effet (délai et boucles compris).

### Module Cinemachine

Avec Cinemachine 3 installé, `CameraImpulseEffect` déclenche une secousse de caméra via le `CinemachineImpulseSource` du même objet. La forme et la durée de la secousse se règlent sur l'Impulse Source ; la Cinemachine Camera doit porter un `CinemachineImpulseListener`. `Stop()` n'interrompt pas une secousse déjà envoyée : Cinemachine ne sait annuler que toutes les secousses à la fois.

### Créer son propre effet

Hériter de `VisualEffectBehaviour` et implémenter `Evaluate` et `ResetEffect`. `TransformOffset` étant interne au package, un effet externe applique son résultat lui-même et doit pouvoir le retirer :

```csharp
using MyUnityPackage.VisualEffect;
using UnityEngine;

public class SpriteBlinkEffect : VisualEffectBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private Color _color = Color.red;

    private Color _restColor;
    private bool _hasRest;

    // Valeurs par défaut propres à cet effet
    public SpriteBlinkEffect()
    {
        SetPlaybackDefaults(duration: 0.5f);
    }

    // Capture de l'état de départ
    protected override void OnPlay()
    {
        if (_sprite == null) return;
        _restColor = _sprite.color;
        _hasRest = true;
    }

    protected override void Evaluate(float progress)
    {
        if (!_hasRest) return;

        // 0 -> 1 -> 0 : part de la couleur d'origine et y revient
        float t = Mathf.Sin(progress * Mathf.PI);
        _sprite.color = Color.Lerp(_restColor, _color, t);
    }

    // Idempotent, et appelé aussi sur un objet détruit : tester les références
    protected override void ResetEffect()
    {
        if (_hasRest && _sprite != null) _sprite.color = _restColor;
        _hasRest = false;
    }
}
```

Points à respecter : `ResetEffect()` est appelé par `Play()` **avant** `OnPlay()` et par `Stop()` ; `OnPlay()` (optionnel) sert à tirer des valeurs aléatoires ou capturer l'état de départ.

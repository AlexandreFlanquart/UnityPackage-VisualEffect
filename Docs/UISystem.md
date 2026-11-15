## 🔄 Système de Transitions

Tous les types sont dans le namespace `MyUnityPackage.VisualEffect`.

### Architecture des Transitions

```
TransitionSO (Base)
├── CustomTransitionSO (Script personnalisé)
│   └── CustomFadeTransitionSO (Fade par script)
└── AnimationTransitionSO (Animation Unity)
    └── AnimationFadeTransitionSO (Fade par animation)
```

### [`TransitionSO.cs`](../Runtime/Core/Scripts/ScriptableObjects/Transitions/TransitionSO.cs)
Classe de base pour toutes les transitions.

```csharp
public abstract class TransitionSO : ScriptableObject
{
    public string transitionName { get; }

    // La Task se termine à la fin de la transition
    public abstract Task PlayTransition(GameObject target);

    // Appelle onProgress (0..1) à chaque frame pendant duration secondes.
    // Retourne false si la cible est détruite ou si on quitte le Play Mode.
    protected static Task<bool> RunOverTime(GameObject target, float duration, bool unscaledTime, Action<float> onProgress = null);
}
```

### [`CustomTransitionSO.cs`](../Runtime/Core/Scripts/ScriptableObjects/Transitions/CustomTransitionSO.cs)
Base pour les transitions personnalisées par script.

```csharp
public abstract class CustomTransitionSO : TransitionSO
{
    protected abstract Task OnCustomTransition(GameObject target);
}
```

### [`CustomFadeTransitionSO.cs`](../Runtime/Core/Scripts/ScriptableObjects/Transitions/CustomFadeTransitionSO.cs)
Fade par script sur un `CanvasGroup` (ajouté à la cible s'il manque).

| Champ | Rôle |
|-------|------|
| `fadeDuration` | Durée d'un fondu, en secondes |
| `mode` | `OutThenIn` (défaut), `In` ou `Out` |
| `useUnscaledTime` | Ignore `Time.timeScale` (défaut : `true`, le fondu fonctionne en pause) |

Exemple de transition personnalisée :

```csharp
[CreateAssetMenu(menuName = "MyUnityPackage/Visual Effect/Transitions/Scale In")]
public class ScaleInTransitionSO : CustomTransitionSO
{
    public float duration = 0.3f;

    protected override Task OnCustomTransition(GameObject target)
    {
        return RunOverTime(target, duration, unscaledTime: true,
            progress => target.transform.localScale = Vector3.one * progress);
    }
}
```

### [`AnimationTransitionSO.cs`](../Runtime/Core/Scripts/ScriptableObjects/Transitions/AnimationTransitionSO.cs)
Base pour les transitions par animation Unity.

```csharp
public abstract class AnimationTransitionSO : TransitionSO
{
    [SerializeField] protected AnimationClip _animationClip;
    [SerializeField] protected RuntimeAnimatorController _animatorController;
    [SerializeField] protected string _stateName = "Default";
}
```

**Fonctionnement :**
- Crée un `AnimatorOverrideController` (une seule fois par asset, partagé entre les cibles)
- Remplace le premier clip du controller par `_animationClip`
- Joue l'état `_stateName` (layer 0) sur l'`Animator` de la cible, ajouté s'il manque
- La `Task` se termine à la fin du clip

### [`AnimationFadeTransitionSO.cs`](../Runtime/Core/Scripts/ScriptableObjects/Transitions/AnimationFadeTransitionSO.cs)
Transition par animation prête à l'emploi. Surchargez `PlayTransition` pour ajouter de la logique avant ou après l'animation :

```csharp
public override async Task PlayTransition(GameObject target)
{
    await base.PlayTransition(target); // terminé quand le clip est fini
    // logique après l'animation
}
```

---

## 🎮 [`UIManager.cs`](../Runtime/Core/Scripts/UIManager.cs)

Gestionnaire statique pour l'UI et les transitions.

### Gestion des Canvas UI

```csharp
// Enregistrer un canvas UI
UIManager.AddCanvasUI<MainMenuUI>(gameObject);

// Récupérer un canvas UI (null s'il n'est pas enregistré ou a été détruit)
MainMenuUI mainMenu = UIManager.GetCanvasUI<MainMenuUI>();
```

### Gestion des Transitions

**Chargement :**
- Les transitions des dossiers `Resources/Transitions/` sont chargées à la première utilisation
- Le `transitionName` sert de clé : il doit être renseigné et unique (sinon l'asset est ignoré avec un warning)

**Utilisation :**

```csharp
// Par nom de transition, sans attendre la fin
UIManager.PlayTransitionByName(canvas, "FadeTransition");

// Par nom de transition, en attendant la fin
await UIManager.PlayTransitionByNameAsync(canvas, "FadeTransition");

// Par trigger d'animator
UIManager.PlayTransitionByTrigger(canvas, animator, "FadeIn");
```

---

## 📁 Structure des Fichiers

Dans le projet qui utilise le package :

```
Assets/
└── Resources/
    └── Transitions/
        ├── FadeTransition.asset (CustomFadeTransitionSO)
        └── SlideTransition.asset (AnimationFadeTransitionSO)
```

---

## ⚙️ Configuration et Création

### 1. Créer un ScriptableObject de Transition

1. Pour un fondu, utilisez directement `Create` → `MyUnityPackage` → `Visual Effect` → `Transitions`. Pour un autre effet, créez un script qui hérite de `CustomTransitionSO` (script) ou `AnimationTransitionSO` (animation).
2. Placez l'asset dans `Resources/Transitions/`
3. Renseignez son `Transition Name` dans l'Inspector

### 2. Configuration du Canvas

`CanvasHelper` et `UI_Base` ci-dessous viennent du package Toolkit, pas de ce package.

1. Créez un GameObject avec un `Canvas`
2. Ajoutez le composant `CanvasHelper` (ajoute automatiquement `CanvasGroup`)
3. Créez un script qui hérite de `UI_Base`
4. Assignez le script au GameObject
5. Configurez `hideOnStart` dans `CanvasHelper` selon vos besoins
6. Enregistrez le canvas dans `UIManager` si nécessaire

```csharp
public class MainMenuUI : UI_Base
{
    void Start()
    {
        // Enregistrer ce canvas dans le UIManager
        UIManager.AddCanvasUI<MainMenuUI>(gameObject);
    }
}
```

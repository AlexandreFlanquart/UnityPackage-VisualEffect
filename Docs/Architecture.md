## 🧱 Structure du projet

Ce document décrit l'organisation du package : dossiers, assemblies, dépendances et conventions. Pour l'usage des effets, voir [Effects.md](Effects.md) ; pour les shaders, [Shaders.md](Shaders.md) ; pour les transitions d'interface, [UISystem.md](UISystem.md).

### Principe

Le package est découpé en **un Core autonome et des modules optionnels**. Le Core s'installe partout ; chaque module ne se compile que si l'outil tiers dont il dépend est présent. Un projet sans DOTween, sans Feel ou sans Cinemachine compile donc sans erreur.

### Arborescence

```
package.json                  Manifeste UPM (nom, version, dépendances, samples)
README.md                     Installation et vue d'ensemble
CHANGELOG.md                  Historique des changements
Docs/                         Documentation par sujet
Runtime/
  Core/                       Assembly principale, toujours compilée
    Scripts/
      3D/                     Effets d'ambiance autonomes (float, move, rotate)
      Effects/                Contrat d'effet, base commune, effets, easing, accessibilité
      ScriptableObjects/
        Transitions/          Transitions d'interface par ScriptableObject
      UIManager.cs            Registre de canvas et lecture des transitions
      AssemblyInfo.cs         Ouvre les types internes au module UGUI
  UGUI/                       Composants qui dépendent de uGUI (EventSystem, Selectable)
  DOTween/                    Module optionnel DOTween
  Feel/                       Module optionnel Feel
  Cinemachine/                Module optionnel Cinemachine
  Shaders/                    Shaders URP fournis
Editor/                       Inspecteur des effets, détection des modules optionnels
Tests/
  Editor/                     Tests Edit Mode
Samples~/
  Demo/                       Démo, importable depuis le Package Manager
```

Le `~` de `Samples~` fait qu'Unity ignore le dossier tant que le sample n'est pas importé. Chaque fichier et dossier hors `Samples~` a un `.meta` versionné : c'est obligatoire dans un package, qui est en lecture seule une fois installé.

### Assemblies

| Assembly | Dossier | Compilée si | Dépend de |
|----------|---------|-------------|-----------|
| `MyUnityPackage.VisualEffect` | `Runtime/Core` | toujours | Toolkit (`MUPLogger`) |
| `MyUnityPackage.VisualEffect.UGUI` | `Runtime/UGUI` | toujours | Core, `UnityEngine.UI` |
| `MyUnityPackage.VisualEffect.DOTween` | `Runtime/DOTween` | define `MUP_DOTWEEN` | Core, `DOTween.Modules` |
| `MyUnityPackage.VisualEffect.Feel` | `Runtime/Feel` | define `MUP_FEEL` | Core, `MoreMountains.Feedbacks`, `MoreMountains.Tools` |
| `MyUnityPackage.VisualEffect.Cinemachine` | `Runtime/Cinemachine` | Cinemachine 3.0+ installé | Core, `Unity.Cinemachine` |
| `MyUnityPackage.VisualEffect.Editor` | `Editor` | éditeur uniquement | Core |
| `MyUnityPackage.VisualEffect.Tests.Editor` | `Tests/Editor` | tests activés | Core, NUnit |

Tous les types runtime sont dans le namespace `MyUnityPackage.VisualEffect`, quel que soit leur module : un seul `using` suffit.

### Activation des modules optionnels

Deux mécanismes, selon l'origine de l'outil tiers :

- **Cinemachine** est un package UPM. Son asmdef utilise un *Version Define* : Unity pose `MUP_CINEMACHINE` tout seul quand `com.unity.cinemachine` 3.0+ est installé.
- **DOTween et Feel** viennent de l'Asset Store, que le Package Manager ne voit pas. Le script [`OptionalModuleDefines.cs`](../Editor/OptionalModuleDefines.cs) cherche leurs assemblies à chaque rechargement des scripts et ajoute ou retire `MUP_DOTWEEN` / `MUP_FEEL` dans les Scripting Define Symbols de la plateforme active. Menu : **Tools > MyUnityPackage > Visual Effect**.

### Dépendances déclarées (`package.json`)

| Dépendance | Raison |
|------------|--------|
| `com.unity.modules.animation` | `Animator` (transitions par animation) |
| `com.unity.modules.ui` | `CanvasGroup` |
| `com.unity.ugui` | Module UGUI |
| `com.unity.render-pipelines.universal` | Shader fourni |

Le package Toolkit n'est pas déclaré ici : il est référencé par GUID dans l'asmdef du Core et doit être installé à la main.

### Architecture des effets

```
IVisualEffect                         Contrat : Play, Stop, IsPlaying, Duration
├── VisualEffectBehaviour             Base : délai, durée, boucles, yoyo, temps non scalé, OnComplete
│   ├── PunchEffect                   ┐
│   ├── PulseEffect                   │ Effets de Transform, additifs
│   ├── ShakeEffect                   │ (via TransformOffset)
│   ├── SquashStretchEffect           │
│   ├── ScaleEffect                   ┘
│   ├── CanvasGroupFadeEffect         Alpha d'un CanvasGroup
│   └── MaterialFloatEffect           Propriété float d'un shader
│       ├── HitFlashEffect            Preset _FlashAmount
│       └── DissolveEffect            Preset _DissolveAmount
└── CameraImpulseEffect               Module Cinemachine
```

Hors de cette hiérarchie, car ils tournent en continu sans notion de lecture : `FloatObject`, `LinearMoveObject`, `RotateObject` (ambiance) et `UIButtonFeedback` (réaction au pointeur).

Briques partagées, dans `Runtime/Core/Scripts/Effects/` :

| Fichier | Rôle |
|---------|------|
| `VisualEffectBehaviour.cs` | Toute la logique de lecture ; une sous-classe n'écrit que `Evaluate(progress)` et `ResetEffect()` |
| `TransformOffset.cs` / `3D/AdditiveOffset.cs` | Mémorisent ce qu'un effet a appliqué pour ne retirer que sa contribution : les effets s'empilent |
| `Easing.cs` | Presets d'easing et courbe personnalisée (`EaseSetting`) |
| `VisualEffectSettings.cs` | Réglages globaux d'accessibilité (`MotionScale`, `FlashScale`) |

### Architecture des transitions d'interface

```
TransitionSO                          Base : nom + PlayTransition(target)
├── CustomTransitionSO                Transition écrite par script
│   └── CustomFadeTransitionSO
└── AnimationTransitionSO             Transition jouée depuis un AnimationClip
    └── AnimationFadeTransitionSO
```

`UIManager` charge les assets de `Resources/Transitions` et les joue par nom. Détail dans [UISystem.md](UISystem.md).

### Conventions de code

- Champs sérialisés privés en `_camelCase` avec `[Tooltip]`, exposés par des propriétés en `PascalCase`.
- Chaque composant a un `[AddComponentMenu("MyUnityPackage/Visual Effect/...")]`.
- Chaque classe a un résumé XML (`/// <summary>`) qui dit à quoi elle sert ; les commentaires dans le code expliquent le pourquoi d'un choix, pas ce que fait la ligne.
- Un effet ne doit jamais laisser sa cible modifiée après `Stop()`, et `ResetEffect()` doit pouvoir être appelé plusieurs fois de suite.
- Pas d'allocation par frame dans le code runtime.

### Où ajouter quoi

| Besoin | Emplacement |
|--------|-------------|
| Nouvel effet sans dépendance | `Runtime/Core/Scripts/Effects/`, en héritant de `VisualEffectBehaviour` (exemple dans [Effects.md](Effects.md)) |
| Composant qui utilise `UnityEngine.UI` ou l'EventSystem | `Runtime/UGUI/Scripts/` |
| Intégration d'un outil tiers | Nouveau dossier `Runtime/<Outil>/` avec sa propre asmdef et un define |
| Shader | `Runtime/Shaders/` |
| Outil d'éditeur | `Editor/` |
| Test | `Tests/Editor/` |

Tout nouveau fichier ou dossier doit être commité avec son `.meta`.

# UnityPackage-VisualEffect

Polish toolkit for Unity: lightweight visual effects for 3D objects, 2D objects and UI.

## Requirements

| Requirement | Notes |
|-------------|-------|
| Unity 6000.0+ | Uses `Awaitable` |
| MyUnityPackage Toolkit | Required. Provides `MUPLogger`, referenced by the Core assembly. Install it before this package. |
| URP (`com.unity.render-pipelines.universal`) | Required by the shipped shader. Declared as a dependency. |
| DOTween | Optional. Only needed for the DOTween module (see below). |

## Install

1. In Unity, open **Window > Package Manager**.
2. Click **+** > **Add package from git URL...**
3. Enter:
   ```
   https://github.com/AlexandreFlanquart/UnityPackage-VisualEffect.git
   ```
   To pin a release, append its tag: `...UnityPackage-VisualEffect.git#v1.0.0`

## Content

All types live in the `MyUnityPackage.VisualEffect` namespace. Components are under **Add Component > MyUnityPackage > Visual Effect**.

### Core (`MyUnityPackage.VisualEffect`)

| Component | Purpose |
|-----------|---------|
| `FloatObject` | Idle pickup animation: floats along an axis with amplitude noise, optional spin |
| `LinearMoveObject` | Sine back-and-forth movement along an axis |
| `RotateObject` | Continuous rotation with optional speed noise |
| `PunchEffect` | One-shot impact (position, rotation or scale) that oscillates back to rest |
| `PulseEffect` | Continuous sine oscillation (breathing, blinking) |
| `ScaleEffect` | Scales between two multipliers of the rest scale (pop in / pop out) |
| `ShakeEffect` | One-shot random shake that fades out |
| `SquashStretchEffect` | Volume-preserving squash and stretch that oscillates back to rest |
| `CanvasGroupFadeEffect` | Fades a `CanvasGroup` alpha, optionally controlling interactivity |
| `MaterialFloatEffect` | Animates any float shader property (by reference name) on one or several renderers |
| `HitFlashEffect` | `MaterialFloatEffect` preset: damage flash (`_FlashAmount` 1 to 0) |
| `DissolveEffect` | `MaterialFloatEffect` preset: dissolve out (`_DissolveAmount` 0 to 1) |

`FloatObject` and `LinearMoveObject` apply their movement as an additive offset: they can be stacked, and combined with gameplay code that moves the same Transform.

All effects share the `IVisualEffect` contract (`Play`, `Stop`, `IsPlaying`, `Duration`) and the playback options of `VisualEffectBehaviour` (delay, loops, yoyo, unscaled time, `OnComplete`). They are additive and can be stacked on the same Transform. When an effect is selected, the Inspector shows **Play** / **Stop** buttons that preview it, in Edit Mode too (the object is restored when the preview ends). See [Docs/Effects.md](Docs/Effects.md).

UI transitions (`TransitionSO`, `UIManager`) are documented in [Docs/UISystem.md](Docs/UISystem.md).

### Shaders

The package ships one URP shader, `MyUnityPackage/Visual Effect/Sprite Effects` (hit flash and dissolve), for `SpriteRenderer` only. [Docs/Shaders.md](Docs/Shaders.md) is a step-by-step Shader Graph tutorial (in French) that explains how to drive shaders with `MaterialFloatEffect` and build your own effects.

### UGUI module (`MyUnityPackage.VisualEffect.UGUI`)

| Component | Purpose |
|-----------|---------|
| `UIButtonFeedback` | Smooth hover / pressed scale feedback for a button (unscaled time), with an optional effect played on click. Respects `Selectable.interactable`. |

This module requires `com.unity.ugui`, which the package declares as a dependency.

### DOTween module (`MyUnityPackage.VisualEffect.DOTween`)

| Component | Purpose |
|-----------|---------|
| `UIFaderAnimator` | Fade in / fade out / blink of a `CanvasGroup`, handles interactivity |

This assembly is only compiled when the `MUP_DOTWEEN` scripting define is set. To enable it:

1. Install DOTween (Asset Store) and run its setup.
2. In **Tools > Demigiant > DOTween Utility Panel**, click **Create ASMDEF** (creates the `DOTween.Modules` assembly this module references).
3. `MUP_DOTWEEN` is then added automatically to the Scripting Define Symbols of the active build target (see *Optional module detection* below).

### Feel module (`MyUnityPackage.VisualEffect.Feel`)

| Feedback | Purpose |
|----------|---------|
| `MMF_VisualEffect` | Plays or stops any effect of this package from an `MMF_Player` (Add new feedback > MyUnityPackage > Visual Effect) |

Requires [Feel](https://feel.moremountains.com) (Asset Store, one licence per seat, never commit it to a repository you deliver). The module is compiled when `MUP_FEEL` is defined, which happens automatically once Feel is imported.

### Cinemachine module (`MyUnityPackage.VisualEffect.Cinemachine`)

| Component | Purpose |
|-----------|---------|
| `CameraImpulseEffect` | Camera shake through a `CinemachineImpulseSource`, with the same `Play()` contract as the other effects |

Compiled automatically when `com.unity.cinemachine` 3.0 or newer is installed. The Cinemachine Camera needs a `CinemachineImpulseListener`.

### Optional module detection

DOTween and Feel are Asset Store assets, so the Package Manager cannot detect them. An editor script looks for their assemblies (`DOTween.Modules`, `MoreMountains.Feedbacks`) each time scripts are reloaded and adds or removes `MUP_DOTWEEN` / `MUP_FEEL` for the **active build target**. This modifies `ProjectSettings/ProjectSettings.asset`.

- Run it by hand: **Tools > MyUnityPackage > Visual Effect > Sync Optional Modules Now** (do it after switching build target).
- Turn it off: untick **Auto Sync Optional Modules** in the same menu, then manage the defines yourself.

## Accessibility

`VisualEffectSettings` holds two global multipliers to wire to your options menu:

```csharp
VisualEffectSettings.MotionScale = 0.3f; // "reduce motion": punch, pulse, shake, squash, float, linear move
VisualEffectSettings.FlashScale = 0f;    // "reduce flashing": HitFlashEffect
```

Only decorative effects are attenuated: `ScaleEffect`, fades and dissolves always reach their end state. Values reset to 1 at the start of each session, so apply the player's saved preference at startup.

## Tests

Edit Mode tests live in `Tests/Editor`. To run them from a project that uses the package, add it to `testables` in `Packages/manifest.json`, then open **Window > General > Test Runner**:

```json
"testables": ["com.myunitypackage.visualeffect"]
```

## Samples

Import **Demo** from the package's **Samples** tab in the Package Manager, then follow its README.

## Troubleshooting

- **`MUPLogger` / `MyUnityPackage.Toolkit` not found**: the Toolkit package is not installed.
- **Compile errors in the Feel or DOTween module after removing the asset**: remove `MUP_FEEL` / `MUP_DOTWEEN` from **Project Settings > Player > Scripting Define Symbols** (the automatic sync cannot run while the project does not compile).
- **`UIFaderAnimator` not available**: `MUP_DOTWEEN` is not defined, or the DOTween ASMDEF has not been created.

Report other issues on the [issue tracker](https://github.com/AlexandreFlanquart/UnityPackage-VisualEffect/issues).

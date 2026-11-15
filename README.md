# UnityPackage-VisualEffect

Polish toolkit for Unity: lightweight visual effects for 3D objects, 2D objects and UI.

## Requirements

| Requirement | Notes |
|-------------|-------|
| Unity 6000.0+ | Uses `Awaitable` |
| MyUnityPackage Toolkit | Required. Provides `MUPLogger`, referenced by the Core assembly. Install it before this package. |
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

`FloatObject` and `LinearMoveObject` apply their movement as an additive offset: they can be stacked, and combined with gameplay code that moves the same Transform.

UI transitions (`TransitionSO`, `UIManager`) are documented in [Docs/UISystem.md](Docs/UISystem.md).

### DOTween module (`MyUnityPackage.VisualEffect.DOTween`)

| Component | Purpose |
|-----------|---------|
| `UIFaderAnimator` | Fade in / fade out / blink of a `CanvasGroup`, handles interactivity |

This assembly is only compiled when the `MUP_DOTWEEN` scripting define is set. To enable it:

1. Install DOTween (Asset Store) and run its setup.
2. In **Tools > Demigiant > DOTween Utility Panel**, click **Create ASMDEF** (creates the `DOTween.Modules` assembly this module references).
3. Add `MUP_DOTWEEN` to **Project Settings > Player > Scripting Define Symbols**.

## Samples

Import **Demo** from the package's **Samples** tab in the Package Manager, then follow its README.

## Troubleshooting

- **`MUPLogger` / `MyUnityPackage.Toolkit` not found**: the Toolkit package is not installed.
- **`UIFaderAnimator` not available**: `MUP_DOTWEEN` is not defined, or the DOTween ASMDEF has not been created.

Report other issues on the [issue tracker](https://github.com/AlexandreFlanquart/UnityPackage-VisualEffect/issues).

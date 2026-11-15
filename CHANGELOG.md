# Changelog

## [Unreleased]

### Changed (breaking)
- All runtime types now live in the `MyUnityPackage.VisualEffect` namespace (3D effects and `UIFaderAnimator` were in the global namespace, transitions in `MyUnityPackage.Toolkit`). Scenes, prefabs and assets keep their references (script GUIDs are unchanged).
- `LinearMoveObjevt` renamed to `LinearMoveObject`.
- `FloatObject`, `LinearMoveObject`, `RotateObject`: public fields are now private serialized fields exposed through PascalCase properties. Serialized values are preserved.
- The DOTween module is now enabled by the `MUP_DOTWEEN` scripting define (was `DOTWEEN_PRESENT`).
- `UIFaderAnimator`: `LoopMode.FadeIn` and `LoopMode.FadeOut` removed (they looped a value onto itself); fades ignore `Time.timeScale` by default (`useUnscaledTime`); `startOnAwake` now replays on every activation.
- Transition assets moved to the `MyUnityPackage/Visual Effect/Transitions` create menu.

### Added
- `CustomFadeTransitionSO`: `mode` (out then in, in, out) and `useUnscaledTime`.
- `AnimationTransitionSO`: configurable state name.
- `UIManager.PlayTransitionByNameAsync`, to await the end of a transition.
- Demo sample.

### Fixed
- DOTween assembly referenced a non-existent `MyUnityPackage.VisualEffect.Core` assembly.
- Sample path in `package.json` pointed to a missing folder; orphan `Editor.meta` and `Plugins.meta` removed.
- `FloatObject` / `LinearMoveObject` overwrote the position every frame and snapped pooled or moved objects back to their first position.
- `FloatObject` did not restore its rotation when disabled.
- `CustomFadeTransitionSO` never reached the final alpha, never ended with `timeScale = 0`, and kept running after the target was destroyed or Play Mode exited.
- `AnimationTransitionSO` completed before the animation ended, leaked an override controller per call, and threw on an unassigned or empty controller.
- `UIManager`: stale static state with Domain Reload disabled, swallowed transition exceptions, misleading `NotImplementedException`, null components registered, empty or duplicate transition names silently overwritten.
- `UIFaderAnimator`: `FadeIn` / `FadeOut` tweens were never tracked, so they could not be killed and overlapped; killing a tween triggered its completion callback.

## [1.0.0] - 2025-03-28
### First Release

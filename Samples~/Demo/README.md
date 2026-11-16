# Demo

The demo is built by a script, so it works in any project without scene or asset to maintain.

1. Import this sample from **Window > Package Manager > UnityPackage-VisualEffect > Samples**.
2. Create a new scene (**File > New Scene**, Basic template: it has a camera and a light).
3. Create an empty GameObject at the origin and add the `VisualEffectDemo` component.
4. Press Play.

| Where | Effects |
|-------|---------|
| Front row | `FloatObject`, `LinearMoveObject`, `RotateObject` |
| Middle row | `PulseEffect`, `PunchEffect`, `ShakeEffect`, `SquashStretchEffect`, `ScaleEffect` (one-shots are looped) |
| Back row | `HitFlashEffect` and `DissolveEffect` on sprites using the `Sprite Effects` shader |
| Bottom of the screen | A button with `UIButtonFeedback` (hover, press) and a `PunchEffect` on click; a panel with `CanvasGroupFadeEffect` |

Each object is labelled with the name of its effect. Select one during Play Mode to tweak it in the Inspector.

Notes:
- The button needs an EventSystem. The demo creates one with the legacy Input Manager; with the Input System package only, add **GameObject > UI > Event System** to the scene.
- Not shown, because they need an optional asset: `CameraImpulseEffect` (Cinemachine), `MMF_VisualEffect` (Feel), `UIFaderAnimator` (DOTween).
- UI transitions (`TransitionSO`) need assets in a `Resources/Transitions` folder: see `Docs/UISystem.md`.

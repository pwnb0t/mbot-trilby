# m'bot Sidekick App (`ownbot-sidekick` repo)

Need to see if there are some opportunities for refactoring now that we are at a good state in the sidekick app.


## Codex understanding and plan

### Refactoring opportunities

1. Extract `TrayController` from `MainWindow`
- Move notify icon lifecycle and tray menu behavior out of `MainWindow.xaml.cs`.
- Own responsibilities:
  - tray icon initialization/disposal
  - menu items (`Show Overlay`, `Hide Overlay`, `Exit`)
  - tray double-click toggle behavior

2. Extract `OverlayController` from `MainWindow`
- Move overlay visibility and interaction-style behavior out of `MainWindow.xaml.cs`.
- Own responsibilities:
  - show/hide state transitions
  - no-activate/transparent interaction toggling
  - bottom reserved strip layout math

3. Evaluate lightweight `OverlayViewModel` after controllers are extracted
- Candidate properties:
  - `IsOverlayVisible`
  - `ClipCountText`
  - `SearchQueryDisplay`
  - `NoResultsVisible`
  - `VisibleClips`
- Only proceed if this clearly reduces imperative UI wiring and shrinks `MainWindow` further.


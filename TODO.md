# m'bot Sidekick App (`ownbot-sidekick` repo)

Need to see if there are some opportunities for refactoring now that we are at a good state in the sidekick app.


## Codex understanding and plan

### Refactoring opportunities

1. Split `MainWindow.xaml.cs` into focused classes
- Current file owns UI composition, overlay visibility state, search state, API orchestration, and tray behavior.
- Refactor into:
  - `OverlayController` (show/hide state, interaction mode, bottom reserved strip)
  - `TrayController` (notify icon + menu actions)

2. Introduce a small ViewModel layer for overlay UI state
- Candidate properties:
  - `IsOverlayVisible`
  - `ClipCountText`
  - `SearchQueryDisplay`
  - `NoResultsVisible`
  - `VisibleClips`
- Reduces imperative UI updates and makes behavior clearer.

3. Add startup/runtime diagnostics wrapper
- Consolidate debug/file log formatting and key lifecycle logs into one logger helper for consistency.

4. Remove/rename legacy "test button" semantics when transitioning to production flow
- Keep functionality but rename internally to production terms (`PinnedClipButtons`, `QuickPlaySlots`) once behavior is finalized.

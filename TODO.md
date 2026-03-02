# m'bot Sidekick App (`ownbot-sidekick` repo)

Need to see if there are some opportunities for refactoring now that we are at a good state in the sidekick app.


## Codex understanding and plan

### Refactoring opportunities

1. Split `MainWindow.xaml.cs` into focused classes
- Current file owns UI composition, overlay visibility state, search state, API orchestration, and tray behavior.
- Refactor into:
  - `OverlayController` (show/hide state, interaction mode, bottom reserved strip)
  - `ClipSearchState` (query, filtering, result selection)
  - `TrayController` (notify icon + menu actions)

2. Replace hard-coded key constants with config-backed key map
- ESC/TAB/ENTER/SPACE are currently fixed in code.
- Create a small `InputBindings` config model for overlay commands (hide, clear search, play first).

3. Introduce a small ViewModel layer for overlay UI state
- Candidate properties:
  - `IsOverlayVisible`
  - `ClipCountText`
  - `SearchQueryDisplay`
  - `NoResultsVisible`
  - `VisibleClips`
- Reduces imperative UI updates and makes behavior clearer.

4. Add lightweight tests for search behavior rules
- Unit-test filter/search behavior independent of WPF:
  - case-insensitive prefix matching
  - 15-result cap
  - TAB clear behavior
  - first-result selection logic

5. Add startup/runtime diagnostics wrapper
- Consolidate debug/file log formatting and key lifecycle logs into one logger helper for consistency.

6. Remove/rename legacy "test button" semantics when transitioning to production flow
- Keep functionality but rename internally to production terms (`PinnedClipButtons`, `QuickPlaySlots`) once behavior is finalized.

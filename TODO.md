# m'bot Sidekick App (`ownbot-sidekick` repo)

Need to see if there are some opportunities for refactoring now that we are at a good state in the sidekick app.


## Codex understanding and plan

### Refactoring opportunities

1. Split `MainWindow.xaml.cs` into focused classes
- Current file owns UI composition, keyboard/mouse hooks, overlay visibility state, search state, API orchestration, and tray behavior.
- Refactor into:
  - `OverlayController` (show/hide state, interaction mode, bottom reserved strip)
  - `OverlayInputRouter` (keyboard/mouse hook handling + key mapping rules)
  - `ClipSearchState` (query, filtering, result selection)
  - `TrayController` (notify icon + menu actions)

2. Move API orchestration out of `MainWindow`
- Keep `SidekickApiClientService` transport-focused and add a `ClipCatalogService` or `ClipPlaybackCoordinator` for app behavior.
- Benefits: easier testing and less UI code-behind coupling.

3. Replace hard-coded key constants with config-backed key map
- ESC/TAB/ENTER/SPACE are currently fixed in code.
- Create a small `InputBindings` config model for overlay commands (hide, clear search, play first).

4. Create reusable style dictionary file(s)
- Move current XAML resources (`ClipButtonStyle`, token brushes, top bar style) into `Styles/` resource dictionaries.
- Keep `MainWindow.xaml` focused on layout, not design token definitions.

5. Centralize design tokens
- Promote spacing/radius/font sizes/colors into named tokens.
- Remove remaining inline values where practical for consistency and future theme changes.

6. Introduce a small ViewModel layer for overlay UI state
- Candidate properties:
  - `IsOverlayVisible`
  - `ClipCountText`
  - `SearchQueryDisplay`
  - `NoResultsVisible`
  - `VisibleClips`
- Reduces imperative UI updates and makes behavior clearer.

7. Add lightweight tests for search behavior rules
- Unit-test filter/search behavior independent of WPF:
  - case-insensitive prefix matching
  - 15-result cap
  - TAB clear behavior
  - first-result selection logic

8. Add startup/runtime diagnostics wrapper
- Consolidate debug/file log formatting and key lifecycle logs into one logger helper for consistency.

9. Remove/rename legacy “test button” semantics when transitioning to production flow
- Keep functionality but rename internally to production terms (`PinnedClipButtons`, `QuickPlaySlots`) once behavior is finalized.

10. Add a “UI constants” module for dimensions
- Current values like search panel width/height and bottom reserved strip are scattered.
- Move to one location to simplify tuning and avoid magic numbers.



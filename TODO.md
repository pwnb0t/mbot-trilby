# m'bot Sidekick App (`ownbot-sidekick` repo)

Need to see if there are some opportunities for refactoring now that we are at a good state in the sidekick app.


## Codex understanding and plan

### Refactoring opportunities

1. Evaluate lightweight `OverlayViewModel` after controllers are extracted
- Candidate properties:
  - `IsOverlayVisible`
  - `ClipCountText`
  - `SearchQueryDisplay`
  - `NoResultsVisible`
  - `VisibleClips`
- Only proceed if this clearly reduces imperative UI wiring and shrinks `MainWindow` further.


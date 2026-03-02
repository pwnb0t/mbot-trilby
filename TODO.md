# m'bot Sidekick App (`ownbot-sidekick` repo)

What's next for Sidekick App?


# Feature - Clip Search/Filter by Typing

## Brain dump

Next feature is implementing the searchable clip feature.
When the user presses the hotkey to open the overlay, they can currently click on a clip to play it.
Additionally, they should be able to start typing alphanumeric chars to begin a search.
As they type, that filters down the search. To start with, it will be very naive and just filter with the beginning of the clip.

Things that must be done in order to implement this:

- Expand the size of the app so that it is full screen
- Keep the logging widget on the bottom for now. It has been helpful.
- Keep the 3 test buttons at the top for now.
- The app needs to load all the clips up using the /v1/clips API on startup
- For now, add a refresh button on the top right that will reload the clips. Also show how many clips are currently loaded. (Not worried about automatic reload or socket events to stay updated for now)
- Add a search widget in the middle, which is detailed below

### Search Widget

Middle of the screen. About 1/3 width of screen in width. Heigth should be approximately 5 "rows" in size.
A "row" height should be the size of a clip button + some padding on bottom and top (a "tile")

To start with, it shows "Start typing to search..." in the top row
Any alphanumeric chars typed will start the search. Anything that is not alphanumeric will be ignored.
When the user starts typing:
* search text replaces the "Start typing to search..." placeholder/CTA (in the top row)
* A list of clip buttons will show below the search box (the 4 rows below the top row)
* The list of clip buttons should be 3 columns and 4 rows
* it will start to display a filtered list of clips below the search widget


## Codex understanding and plan

### Understanding
- This feature is a keyboard-first clip picker layered on top of the current overlay.
- On overlay open, Sidekick should already have a local in-memory list of clips fetched from GET /v1/clips.
- Typing alphanumeric characters builds a search string; non-alphanumeric keys are ignored.
- Filtering is prefix-only for now (naive starts-with, case-insensitive).
- The search panel in the center should show:
  - Row 1: placeholder ("Start typing to search...") or current query.
  - Rows 2-5: filtered results as a 3x4 grid of clip buttons.
- Existing top controls (3 test buttons, plus new Refresh and clip count) and bottom log area should remain.
- Overlay should become fullscreen for this iteration.

### Plan (pre-implementation)
1. Confirm UI behavior contracts
- Confirm exact key handling while overlay is visible: character keys, Backspace, Escape, Enter.
- Confirm typing is captured while overlay is visible (keyboard-first behavior).

2. Add clip-loading state and refresh flow
- Add a clip cache service/state in the WPF app (allClips, filteredClips, lastRefreshAt, loading/error state).
- On app startup, call /v1/clips and populate allClips.
- Add top-right Refresh button to re-fetch clips.
- Show loaded clip count near refresh control.
- Log load/refresh success/failure to UI + file log.

3. Expand overlay layout for fullscreen and new regions
- Keep top row with existing 3 test buttons.
- Add refresh + count on top-right.
- Keep bottom logging panel unchanged.
- Add centered search widget sized to roughly 1/3 screen width and ~5 tile rows height.

4. Implement search input pipeline
- Capture preview keyboard input when overlay is visible.
- Accept only alphanumeric chars into searchQuery.
- Ignore non-alphanumeric input.
- Enter or Space plays the first filtered result when query is non-empty.
- Backspace removes last char.
- Empty query restores placeholder and clears result grid.
- Escape clears the search query; if already empty, Escape hides the overlay.

5. Implement filtering and result grid
- Build filtered list from allClips using prefix match on trigger only.
- Render up to 12 results in a 3x4 grid.
- Each result tile/button plays the selected clip through existing API flow.
- If no matches, show a lightweight empty-state message in results area.

6. Verification and guardrails
- Verify focus behavior in-game remains non-activating (no regression).
- Verify clip play still works from existing test buttons.
- Verify refresh updates count and search results.
- Verify log output for: load, refresh, search input, clip play success/failure.

### Open questions
- None at this stage. Behavior decisions are confirmed and implementation-ready.

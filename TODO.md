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

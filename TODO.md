# m'bot Sidekick app

Transparent overlay to be able to play clips through m'bot on the discord server.
    This is NOT playing it through the user's mic (via a virtual wire or anything like that)
    It sends a command to m'bot and plays from m'bot to the discord channel that he is currently in.
        Similar to if a user sent `!play hello` to the #bot-text channel (the normal way clips are played today)

Ideally, there would be a hotkey, like ALT+` to show overlay
    clicking a particular clip would play it
        or releasing hotkey will play the clip?
    Similar to the radial wheel in the built-in discord soundboard overlay.
    I think I would like to have a more complex view than the radial wheel overlay

## Phase 1

Get a transparent overlay with some test buttons.
Pressing those buttons will print to log
Be able to press hotkey to show overlay while in a game

## Phase 2

Connect to test bot
No need for full auth yet, just a POC connection
Pressing a button will send a command to test bot to play a clip

## Phase 3

Auth.
User either needs to send message to the bot to get a pin/key to put in the overlay or something along these lines.
It might need to be per-server (per-guild).
Might need to be able to select the server you're on.


## Phase 4

Searchable list of clips from server


## more ideas about profiles and stuff (probaby phase 4+ things)
Per server profiles
Per game profiles
Shared profiles (per server per game)
    e.g. siege profile in RZDZ server that lists all the operator clips


-----

I am now trying to test the app while in a game (Rocket League).
I am going to compare with Discord's overlay features. Discord has 3 different overlay features. They only work if the game is set to borderless (or windowed) and not fullscreen. Borderless is what I normally run, so that's fine. Here are the 3 different overlay features:
1. Channel List - There is a channel listing that always shows while in game. It will light up a user's name when they're talking. In RL, this has no effect on the game.
2. Main Overlay - pressing a key command (shift+\` maybe? mine is rebound to ctrl+shift+\`), will open the "main" overlay which has voice/mute/deafen settings, where you can change the overlay for (1), notification settings, etc. In RL, when this is activate, RL is muted like it loses focus (same as if I alt-tab). But I can still see RL behind the transparent overlay (though it is darkened just a bit).
3. Soundboard Radial - they have a radial menu for doing soundboard stuff. It requires a key command (ctrl+\` by default). Almost no effect on RL when activated. RL does not lose focus or sound, and it does not darken. You can mouse over the option in the radial menu and when you let go of ctrl+\` it plays the sound.

For Sidekick, when I hit the alt+` key combination, the in-game music stops (like it loses focus), but I can see the overlay window. This matches the behavior of the "main" Discord overlay (like if I hit the key combination to bring up voice settings and what not). I would  prefer if it more matched the Soundboard Radial and the user did not lose in-game sound. But I don't know if that is a possibility.


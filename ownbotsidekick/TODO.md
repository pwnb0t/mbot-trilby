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


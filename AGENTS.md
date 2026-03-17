# ownbotsidekick / Trilby

Originally codenamed "sidekick" but is now named Trilby

1. Use Windows line endings (CRLF) for all text/code files.
2. Build and run from Windows/Visual Studio, not WSL.
3. Keep overlay non-activating in-game: use `WS_EX_NOACTIVATE` path and avoid focus-stealing calls.
4. Do not use `ShowActivated=False` on this window config (causes WPF runtime exception).
5. Keep hotkey + tray behavior intact: `RegisterHotKey` toggle, unregister on close, close button hides to tray, tray menu includes `Show Overlay`, `Hide Overlay`, `Exit`.
6. Keep tray icon sourced from root `mbot.ico` copied to output; fallback to system icon if missing.
7. Keep logging to UI + Debug + `%LocalAppData%\\ownbotsidekick\\logs\\overlay.log`.
8. Keep `appsettings.json` as the runtime config source for overlay and Sidekick API settings; preserve safe defaults.
9. Treat `E:\\g\\ownbot\\openapi\\sidekick.v1.yaml` as API source of truth; regenerate SDK with `scripts\\generate-sidekick-sdk.bat` after contract changes.
10. Discord snowflake fields such as `guild_id` and `requester_user_id` must remain `long`/`long?` in the generated SDK; if they regress to `int`, fix the OpenAPI contract generation instead of adding casts in Trilby.

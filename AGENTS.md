# ownbotsidekick / Trilby

Originally codenamed "sidekick" but is now named Trilby

- Use Windows line endings (CRLF) for all text/code files.
- Build and run from Windows/Visual Studio, not WSL.
- Keep overlay non-activating in-game: use `WS_EX_NOACTIVATE` path and avoid focus-stealing calls.
- Do not use `ShowActivated=False` on this window config (causes WPF runtime exception).
- Keep hotkey + tray behavior intact: `RegisterHotKey` toggle, unregister on close, close button hides to tray, tray menu includes `Show Overlay`, `Hide Overlay`, `Exit`.
- Keep tray icon sourced from root `mbot.ico` copied to output; fallback to system icon if missing.
- Keep logging to UI + Debug + `%LocalAppData%\\ownbotsidekick\\logs\\overlay.log`.
- Keep user settings in `%LocalAppData%\\ownbotsidekick\\user-settings.json`; this is Trilby's persisted per-user state for quick play assignments, selected tag state, and similar local preferences.
- Keep `appsettings.json` as the runtime config source for overlay and Sidekick API settings; preserve safe defaults.
- Treat `E:\\g\\ownbot\\openapi\\sidekick.v1.yaml` as API source of truth; regenerate SDK with `scripts\\generate-sidekick-sdk.bat` after contract changes.
- If an expected API method is missing from the generated SDK, assume stale generated output first: verify the source OpenAPI in `ownbot`, rerun SDK generation, and confirm the generated files actually contain the operation before adding any manual `HttpClient` workaround.
- Discord snowflake fields such as `guild_id` and `requester_user_id` must remain `long`/`long?` in the generated SDK; if they regress to `int`, fix the OpenAPI contract generation instead of adding casts in Trilby.

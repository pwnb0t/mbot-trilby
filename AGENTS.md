# mbot-trilby / Trilby

Originally used a different codename and now named Trilby

- Use Windows line endings (CRLF) for all text/code files.
- Build and run from Windows/Visual Studio, not WSL.
- Keep overlay non-activating in-game: use `WS_EX_NOACTIVATE` path and avoid focus-stealing calls.
- Do not use `ShowActivated=False` on this window config (causes WPF runtime exception).
- Keep hotkey + tray behavior intact: `RegisterHotKey` toggle, unregister on close, close button hides to tray, tray double-click opens Settings, and the tray menu keeps Settings/Exit behavior stable.
- Keep tray icon sourced from root `mbot.ico` copied to output; fallback to system icon if missing.
- Keep logging to UI + Debug + `%LocalAppData%\\mbot-trilby\\logs\\overlay.log`.
- Keep user settings in `%LocalAppData%\\mbot-trilby\\user-settings.json`; this is Trilby's persisted per-user state for quick play assignments, selected tag state, selected environment, and per-environment auth sessions.
- Keep `appsettings.json` as the runtime config source for overlay and Trilby environment URLs; preserve safe defaults.
- Treat `openapi\\trilby.v1.yaml` in the sibling `mbot` repo as API source of truth; regenerate SDK with `scripts\\generate-trilby-sdk.bat` after contract changes.
- If an expected API method is missing from the generated SDK, assume stale generated output first: verify the source OpenAPI in the sibling `mbot` repo, rerun SDK generation, and confirm the generated files actually contain the operation before adding any manual `HttpClient` workaround.
- Discord snowflake fields such as `guild_id` and `requester_user_id` must remain `long`/`long?` in the generated SDK; if they regress to `int`, fix the OpenAPI contract generation instead of adding casts in Trilby.
- Trilby user identity now comes from bearer auth; do not add client-supplied `requester_user_id` parameters back into Trilby API calls.
- In authored C# code, prefer explicit local variable types instead of `var` when the type can be written directly. Leave generated SDK code alone unless explicitly requested.
- Avoid opaque relational pattern negation such as `if (guildId is not > 0)`. Prefer clearer nullable/value checks such as `if (guildId is null or <= 0)` or `if (guildId is null || guildId <= 0)`.

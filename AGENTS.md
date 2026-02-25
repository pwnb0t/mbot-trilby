# ownbotsidekick

1. Use Windows line endings (CRLF) for all text/code files.
2. Build and run from Windows/Visual Studio, not WSL.
3. Keep overlay non-activating in-game: use WS_EX_NOACTIVATE path and avoid focus-stealing calls.
4. Do not use `ShowActivated=False` on this window config (causes WPF runtime exception).
5. Keep global hotkey toggle (`RegisterHotKey`) and unregister on close.
6. Preserve tray flow: `Show Overlay`, `Hide Overlay`, `Exit`; close button should hide to tray.
7. Keep tray icon sourced from root `mbot.ico` copied to output; fallback to system icon if missing.
8. Keep logging to UI + Debug + `%LocalAppData%\\ownbotsidekick\\logs\\overlay.log`.
9. Maintain `appsettings.json` copy-to-output and safe defaults for hotkey/overlay behavior.
10. Update `STATUS.md` after meaningful behavior or architecture changes.

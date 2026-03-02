# ownbotsidekick Status

See E:\g\ownbot for the original project
E:\g\ownbot\TODO.md also has a section about this Sidekick app.

## Snapshot
- Date: 2026-02-25
- Phase: Sidekick app Phase 1 (overlay proof-of-concept)
- Environment note: build/run should be done from Windows/Visual Studio (not WSL).

## Phase 2 Progress (API SDK Integration)
- Date: 2026-03-02
- Bumped OpenAPI contract version from `0.0.2` to `0.0.3` in `E:\g\ownbot\openapi\sidekick.v1.yaml`.
- Regenerated C# SDK from `E:\g\ownbot\openapi\sidekick.v1.yaml` using `scripts\generate-sidekick-sdk.bat`.
- SDK contract now includes:
  - `PlayClip` (`POST /v1/clips/play`)
  - `ListClips` (`GET /v1/clips`)
- Updated app client integration to new SDK names:
  - `PlayTriggerRequest` -> `PlayClipRequest`
  - `_api.PlayTriggerAsync(...)` -> `_api.PlayClipAsync(...)`
  - `SidekickApiClientService.PlayTriggerAsync(...)` -> `PlayClipAsync(...)`
- Verified `dotnet build ownbotsidekick\ownbotsidekick.csproj -c Debug` succeeds (0 errors).

## Phase 2 Progress (Clip Search/Filter UX)
- Date: 2026-03-02
- Expanded overlay layout for fullscreen usage while preserving:
  - top test clip buttons
  - bottom log panel
  - tray flow and no-activate behavior
- Added clip catalog load from `GET /v1/clips` on startup.
- Added top-right `Refresh` button and loaded clip count/status display.
- Added center search widget:
  - placeholder: `Start typing to search...`
  - 3 columns x 4 rows results grid
  - prefix filtering, case-insensitive, trigger-only
- Added keyboard behavior (while overlay visible):
  - alphanumeric appends to search query
  - non-alphanumeric ignored
  - `Backspace` deletes one character
  - `Enter` or `Space` plays first filtered result (if query is non-empty)
  - `Escape` clears search; if already empty, hides overlay
- Added non-blocking startup behavior if clip load fails:
  - overlay still opens
  - user can retry with `Refresh`
- Verified `dotnet build ownbotsidekick\ownbotsidekick.csproj -c Debug` succeeds (0 errors, 0 warnings).

## What Is Implemented
- Transparent, topmost WPF overlay window.
- 3 clickable buttons (`Clip A`, `Clip B`, `Clip C`).
- Button clicks log to:
  - Visual Studio Debug output
  - On-screen log textbox
  - File log at `%LocalAppData%\\ownbotsidekick\\logs\\overlay.log`
- Global hotkey support using `RegisterHotKey` to show/hide overlay.
- Non-activating overlay behavior for in-game use (WS_EX_NOACTIVATE + no `Activate()` calls), validated in Rocket League borderless mode with game audio preserved while overlay is shown.
- System tray icon flow:
  - Tray icon is always available while app runs.
  - Uses `mbot.ico` (generated from `mbot-square-cropped.png` and copied to build output); falls back to default app icon if icon is unavailable.
  - Tray menu supports `Show Overlay`, `Hide Overlay`, and `Exit`.
  - Closing window (`X` / Alt+F4) hides to tray instead of exiting.
- JSON config (`appsettings.json`) for hotkey and overlay startup behavior:
  - `Hotkey.Modifiers` (default `Alt`)
  - `Hotkey.Key` (default `Oem3`, backtick key on US layout)
  - `Overlay.StartHidden` (default `false`)
  - `Overlay.Topmost` (default `true`)

## Files Changed
- `ownbotsidekick/MainWindow.xaml`
  - Borderless overlay window config and overlay panel UI (buttons + log textbox).
- `ownbotsidekick/MainWindow.xaml.cs`
  - Button click handlers and `Log(string)` helper.
  - Startup logging.
  - Global hotkey register/unregister and WM_HOTKEY handling.
  - Overlay visibility toggle and appsettings loading/parsing.
  - Tray icon initialization, tray menu actions, and close-to-tray behavior.
  - Non-activating overlay handle style (`WS_EX_NOACTIVATE`).
- `ownbotsidekick/App.xaml.cs`
  - `App` base type explicitly set to `System.Windows.Application` (avoids WinForms ambiguity).
- `ownbotsidekick/appsettings.json`
  - Default hotkey/overlay config.
- `ownbotsidekick/ownbotsidekick.csproj`
  - Copy-to-output for `appsettings.json`.
  - `<UseWindowsForms>true</UseWindowsForms>` for tray support.
  - Copy-to-output for root `mbot.ico` as linked file.
- `.gitattributes` and `.editorconfig`
  - Enforced CRLF line endings for VS consistency.

## Build/Run Verification (PowerShell)
```powershell
dotnet build E:\g\ownbotsidekick\ownbotsidekick\ownbotsidekick.csproj -nologo
dotnet build E:\g\ownbotsidekick\ownbotsidekick\ownbotsidekick.csproj -c Release -nologo
```
Both builds pass with 0 errors and 0 warnings as of 2026-02-25.

Then run via Visual Studio and test:
- Click each button.
- Press `Alt+`` to hide/show overlay.
- Use tray icon menu to show/hide and `Exit`.
- Close window and confirm app remains in tray.
- In Rocket League (borderless), confirm overlay toggles without stealing focus/audio.

## Next Suggested Steps
1. Finalize Phase 1 acceptance checklist.
2. Define Phase 2 scope (overlay interactions and clip workflow).
3. Decide whether to add hold-to-show radial mode.

## Notes
- Solution file appears as `.slnx` in VS 2026 (`ownbotsidekick.slnx`).
- Project target is `net10.0-windows` with `<UseWPF>true</UseWPF>`.

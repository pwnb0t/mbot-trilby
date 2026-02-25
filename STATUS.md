# ownbotsidekick Status

See E:\g\ownbot for the original project
E:\g\ownbot\TODO.md also has a section about this Sidekick app.

## Snapshot
- Date: 2026-02-25
- Phase: Sidekick app Phase 1 (overlay proof-of-concept)
- Environment note: last edits were made from WSL path `/mnt/e/g/ownbotsidekick`; build/run should be done from Windows/Visual Studio.

## What Is Implemented
- Transparent, topmost overlay window in WPF.
- 3 clickable buttons (`Clip A`, `Clip B`, `Clip C`).
- Button clicks log to:
  - Visual Studio Debug output
  - On-screen log textbox
  - File log at `%LocalAppData%\\ownbotsidekick\\logs\\overlay.log`
- Global hotkey support using `RegisterHotKey` to show/hide overlay.
- System tray icon flow:
  - Tray icon is always available while app runs.
  - Tray menu supports `Show Overlay`, `Hide Overlay`, and `Exit`.
  - Closing window (`X` / Alt+F4) now hides to tray instead of exiting.
- JSON config (`appsettings.json`) for hotkey and overlay startup behavior:
  - `Hotkey.Modifiers` (default `Alt`)
  - `Hotkey.Key` (default `Oem3`, backtick key on US layout)
  - `Overlay.StartHidden` (default `false`)
  - `Overlay.Topmost` (default `true`)

## Files Changed
- `ownbotsidekick/MainWindow.xaml`
  - Window configured as borderless overlay (`WindowStyle=None`, `AllowsTransparency=True`, `Topmost=True`, `WindowState=Maximized`, etc.)
  - Added overlay panel UI, 3 buttons, and log textbox.
- `ownbotsidekick/MainWindow.xaml.cs`
  - Added click handlers for all 3 buttons.
  - Added `Log(string)` helper.
  - Added startup logging in `Window_Loaded`.
  - Added global hotkey registration/unregistration and WM_HOTKEY handling.
  - Added overlay visibility toggle and appsettings loading/parsing.
  - Added tray icon initialization, tray menu actions, and close-to-tray behavior.
- `ownbotsidekick/App.xaml.cs`
  - Updated `App` base type to `System.Windows.Application` to avoid ambiguity after enabling WinForms.
- `ownbotsidekick/appsettings.json`
  - Added default hotkey/overlay behavior config.
- `ownbotsidekick/ownbotsidekick.csproj`
  - Added copy-to-output for `appsettings.json`.
  - Enabled `<UseWindowsForms>true</UseWindowsForms>` for tray icon support.

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

Expected behavior:
- Overlay appears full-screen transparent with a visible control panel.
- Clicking each button appends timestamped log lines in UI and log file.
- Hotkey toggles overlay visibility even when overlay is hidden.
- Tray icon can reopen overlay and cleanly exit app.

## Next Suggested Steps (Phase 1 completion)
1. Confirm hotkey behavior on the target machine keyboard layout.
2. Confirm startup/run behavior in Visual Studio on target machine.
3. Decide whether to add tray icon exit/reopen flow before Phase 2.

## Notes
- Solution file appears as `.slnx` in VS 2026 (`ownbotsidekick.slnx`).
- Project target is `net10.0-windows` with `<UseWPF>true</UseWPF>`.

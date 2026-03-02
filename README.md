# ownbotsidekick

See E:\g\ownbot for the original project
E:\g\ownbot\TODO.md also has a section about this Sidekick app.

## Build/Run Verification (PowerShell)
```powershell
dotnet build E:\g\ownbotsidekick\ownbotsidekick\ownbotsidekick.csproj -nologo
dotnet build E:\g\ownbotsidekick\ownbotsidekick\ownbotsidekick.csproj -c Release -nologo
```
Both builds pass with 0 errors and 0 warnings as of 2026-02-25.

## Notes
- Solution file appears as `.slnx` in VS 2026 (`ownbotsidekick.slnx`).
- Project target is `net10.0-windows` with `<UseWPF>true</UseWPF>`.

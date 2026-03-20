# mbot-trilby

See the paired `mbot` repo for the server/API side.
That repo's `TODO.md` also has a section about this Trilby app.

## Build/Run Verification (PowerShell)
```powershell
dotnet build E:\g\mbot-trilby\mbot-trilby\mbot-trilby.csproj -nologo
dotnet build E:\g\mbot-trilby\mbot-trilby\mbot-trilby.csproj -c Release -nologo
```
Both builds pass with 0 errors and 0 warnings as of 2026-02-25.

## Notes
- Solution file appears as `.slnx` in VS 2026 (`mbot-trilby.slnx`).
- Project target is `net10.0-windows` with `<UseWPF>true</UseWPF>`.

# mbot-trilby

See the paired `mbot` repo for the server/API side.
That repo's `TODO.md` also has a section about this Trilby app.

## Build

From the repo root:

```powershell
dotnet build .\mbot-trilby\mbot-trilby.csproj -nologo
dotnet build .\mbot-trilby\mbot-trilby.csproj -c Release -nologo
```

Run tests:

```powershell
dotnet test .\mbot-trilby.Tests\mbot-trilby.Tests.csproj -nologo
```

## Notes
- Solution file appears as `.slnx` in VS 2026 (`mbot-trilby.slnx`).
- Project target is `net10.0-windows` with `<UseWPF>true</UseWPF>`.

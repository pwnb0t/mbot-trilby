# Status

## 2026-03-16

- Added Trilby tag catalog loading on startup and manual refresh.
- Search now supports mixed clip/tag results, including `&tag` input and the requested clip-before-tag ordering.
- Added Sidekick tag read endpoints to `ownbot` and refreshed the checked-in OpenAPI YAML copy in `ownbotsidekick`.
- The generated C# SDK was not fully regenerated on this machine because the repo script depends on Docker and local `npx` generation also required Java, which is not installed here.

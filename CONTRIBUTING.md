# Contributing to Plugin.Firebase

Thanks for contributing! This repo is public OSS. Please keep contributions general and reusable.

## Design Philosophy: Thin Wrapper

Plugin.Firebase aims to be a thin, cross-platform wrapper around the native Firebase SDKs (via iOS/Android binding packages). When adding or changing APIs, prefer direct exposure of native concepts over higher-level convenience helpers.

**Goals:**
- Developers should rely on Firebase's official documentation without learning a separate ".NET Firebase" abstraction.
- Public surface area remains predictable and unsurprising.
- Maintenance and testing costs stay low by avoiding additional business logic ownership.

## Quick start
1) Fork the repo and create a feature branch.
2) Make focused, scoped changes aligned with the design philosophy.
3) Run formatting and relevant builds (see [Local Validation](#local-validation)).
4) Update docs if APIs change.
5) Add/adjust tests for behavior changes.
6) Open a PR with a clear description.

## Development guidelines

### Code style & format
- Follow `.editorconfig` rules.
- Format code before PRs: `dotnet format Plugin.Firebase.sln`
- Never commit secrets (Firebase configs, signing keys, tokens).
- Avoid app-specific or proprietary context in code and docs.

### API design principles
- **Prefer 1:1 mappings** of native properties and methods. Use only light .NET conventions (PascalCase, `Async` suffix).
- **Avoid convenience helpers** that combine multiple native operations or hide intent.
- **Avoid new abstractions** like enums, error codes, policy layers.
- **Avoid "helpful" normalization** like trimming, treating whitespace as null, default fallbacks.
- **Document platform differences** explicitly in XML docs and component docs when iOS and Android APIs differ.
- **Prefer pass-through behavior** and leave app-specific policy decisions to the consumer app.

### Backward compatibility
- Keep API changes backward-compatible when possible.
- If breaking changes are necessary, document them clearly in the PR.

## Versioning

Package versions are defined per project (`src/*/*.csproj` as `PackageVersion`).

In general, bump only the package(s) whose public surface changed. A major version bump to `Plugin.Firebase.Auth` does **not** automatically mean unrelated packages (for example `Plugin.Firebase.Firestore`) must be re-released.

Maintainers may prefer to handle version bumps as part of the release process. If you're unsure, leave versions unchanged and call it out explicitly in the PR.

## Testing & Validation

### Local validation
At minimum, validate compilation for the relevant targets:

```sh
dotnet build src/Auth/Auth.csproj -f net9.0
dotnet build src/Auth/Auth.csproj -f net9.0-android
dotnet build src/Auth/Auth.csproj -f net9.0-ios
```

If you change APIs used by integration tests:

```sh
dotnet build tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj
```

### Integration testing
Integration tests require a real device and local Firebase config files. See `BUILDING.md` for setup and test commands.

### Manual testing with Playground sample
Use the [Playground](sample/Playground) sample app for manual integration testing and exploratory validation on device/simulator. The app demonstrates real-world Firebase API usage and serves as a live integration test ground. See `sample/README.md` and `BUILDING.md` for setup and deployment instructions.

## Formatting modified files

Format all files changed on the current branch (compared to `origin/development`):
```sh
dotnet format Plugin.Firebase.sln --include $(git diff --name-only origin/development...HEAD)
```

Format only staged files (not yet committed):
```sh
dotnet format Plugin.Firebase.sln --include $(git diff --name-only --cached)
```

## PR Expectations

- **Keep PRs focused**: one feature or fix per PR.
- **Update docs** when public API changes (XML docs and `docs/*.md`).
- **Add/adjust tests** for behavior changes:
  - Unit tests for app-logic or adapters.
  - Integration tests for "does not throw" and basic native interop behavior.
- **Run local validation** before opening a PR.

## Documentation

If you change behavior or APIs, update the relevant docs:
- **Component docs**: `docs/*.md`
- **README**: `README.md`
- **XML docs**: Code comments (shown in IntelliSense)

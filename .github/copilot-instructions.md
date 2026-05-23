# Copilot instructions

This repository is public open source. Keep contributions generic and reusable.
Do **not** include app-specific business context, private data, secrets, or tokens.

## Scope
- Prefer small, reviewable, non-breaking changes.
- Avoid changing public APIs unless explicitly required and documented.
- If you change behavior or APIs, update the relevant docs in `docs/` and/or `README.md`.

## Build & test
- Use the .NET SDK version from `global.json`.
- Format before PRs: `dotnet format Plugin.Firebase.sln`
- Unit tests: `dotnet test tests/Plugin.Firebase.UnitTests/Plugin.Firebase.UnitTests.csproj`

## Platform notes
Most projects are multi-targeted (`net9.0`, `net9.0-android`, `net9.0-ios`).
Prefer validating `net9.0` in CI unless platform-specific changes require more.

## Integration tests
- Before editing `tests/Plugin.Firebase.IntegrationTests`, read `tests/Plugin.Firebase.IntegrationTests/README.md`.
- Use `tests/Plugin.Firebase.IntegrationTests/ACCEPTANCE_COVERAGE.md` as the package coverage checklist.
- Keep emulator-backed coverage automatic and real-backend, destructive, paid, or externally delivered checks opt-in.
- Run `scripts/check-integration-coverage.rb` after adding, renaming, moving, or splitting fixtures.

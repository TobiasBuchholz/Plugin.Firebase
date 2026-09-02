# Plugin.Firebase — AI Collaboration Guide

This repository is public open source. Keep all contributions generic and reusable.
Do **not** include private app details, proprietary business context, or secrets.

**See [CONTRIBUTING.md](CONTRIBUTING.md) for the complete contribution guide**, including design philosophy, API principles, versioning, and PR expectations.

## Repository map
- `src/`: Package implementations (multi-targeted `net10.0`, `net10.0-android`, `net10.0-ios`)
- `docs/`: Feature docs and setup guides
- `sample/`: MAUI sample app
- `tests/`: Integration test harness (runs on device)

## Key guardrails
- **Thin wrapper philosophy**: Favor 1:1 mappings of native APIs. See [Design Philosophy](CONTRIBUTING.md#design-philosophy-thin-wrapper).
- Follow `.editorconfig` formatting rules; run `dotnet format Plugin.Firebase.sln` before PRs.
- Never commit secrets (Firebase config files, signing keys, tokens).
- Keep changes minimal and scoped; avoid app-specific assumptions.
- Update docs and tests for behavior changes.

## Quick build & test
- Restore: `dotnet restore Plugin.Firebase.sln`
- Build (all TFMs): `dotnet build Plugin.Firebase.sln -c Release`
- Build only `net10.0` (no mobile workloads): `dotnet build src/Auth/Auth.csproj -c Release -f net10.0 -p:TargetFrameworks=net10.0`
- Unit tests: `dotnet test tests/Plugin.Firebase.UnitTests/Plugin.Firebase.UnitTests.csproj`
- Integration coverage audit: `scripts/check-integration-coverage.rb`
- Integration tests are device-only. See `BUILDING.md`.

## AI workflow
1) **Understand the scope**: Read the relevant docs in `docs/` plus the target feature area in `src/`.
2) **Reference design principles**: Review [Design Philosophy](CONTRIBUTING.md#design-philosophy-thin-wrapper) and [API design principles](CONTRIBUTING.md#api-design-principles).
3) **Propose change set**: Design a small, verifiable change before editing code.
4) **Keep it documented**: Ensure public API changes are explicit and documented per [Documentation](CONTRIBUTING.md#documentation).

## Integration test workflow
- Before changing `tests/Plugin.Firebase.IntegrationTests`, read `tests/Plugin.Firebase.IntegrationTests/README.md` and `tests/Plugin.Firebase.IntegrationTests/ACCEPTANCE_COVERAGE.md`.
- Keep tests close to native Firebase behavior; avoid app-specific policy, private project details, or hidden real-backend assumptions.
- Use the harness fact attributes, fixture metadata, resource scopes, and file layout documented in the integration test README.
- Update acceptance coverage and real-backend setup docs when public API coverage or backend requirements change.
- Run `scripts/check-integration-coverage.rb` after adding, renaming, moving, or splitting fixtures.

## Packaging & CI
- Packaging guidelines: `docs/packaging-github-packages.md`
- CI guidance: `docs/ci-cd.md`
- Workflows: `.github/workflows/ci.yml`, `.github/workflows/publish-github-packages.yml`

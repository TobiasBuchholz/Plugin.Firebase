# Plugin.Firebase — AI Collaboration Guide

This repository is public open source. Keep all contributions generic and reusable.
Do **not** include private app details, proprietary business context, or secrets.

## Repository map
- `src/`: Package implementations (multi-targeted `net9.0`, `net9.0-android`, `net9.0-ios`)
- `docs/`: Feature docs and setup guides
- `sample/`: MAUI sample app
- `tests/`: Integration test harness (runs on device)

## Guardrails
- Follow `.editorconfig` formatting rules.
- Run `dotnet format src/Plugin.Firebase.sln` before PRs.
- Avoid committing secrets (Firebase config files, signing keys, tokens).
- Keep changes minimal and scoped; update docs when behavior changes.
- Do not introduce app-specific assumptions.

## Build & test (quick)
- Restore: `dotnet restore Plugin.Firebase.sln`
- Build (all TFMs): `dotnet build Plugin.Firebase.sln -c Release`
- Build only `net9.0` (no mobile workloads): `dotnet build src/Auth/Auth.csproj -c Release -f net9.0`
- Tests are integration/device-only. See `BUILDING.md`.

## Packaging & CI
- Packaging guidelines: `docs/packaging-github-packages.md`
- CI guidance: `docs/ci-cd.md`

## AI workflow
1) Read the relevant docs in `docs/` plus the target feature area in `src/`.
2) Propose a small, verifiable change set before editing code.
3) Keep public API changes explicit and documented.

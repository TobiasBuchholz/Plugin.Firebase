# Contributing to Plugin.Firebase

Thanks for contributing! This repo is public OSS. Please keep contributions general and reusable.

## Quick start
1) Fork the repo and create a feature branch.
2) Make focused changes.
3) Run formatting and relevant builds.
4) Open a PR with a clear description.

## Development guidelines
- Follow `.editorconfig` rules.
- Format code before PRs: `dotnet format Plugin.Firebase.sln`
- Avoid app-specific or proprietary context in code/docs.
- Never commit secrets (Firebase configs, signing keys, tokens).
- Keep API changes backward-compatible when possible.

## Versioning
Package versions are defined per project (`src/*/*.csproj` as `PackageVersion`).
If you change package APIs, ensure the version bump is consistent across affected packages.

### Release process (recommended)
This repo uses lockstep package versioning (all `src/*/*.csproj` share the same `PackageVersion`).

When preparing a release:
1) Bump `PackageVersion` in **all** projects under `src/*/*.csproj`.
   - Use **patch** (`4.0.0` → `4.0.1`) for bugfixes and backward-compatible improvements.
   - Use **minor** (`4.0.0` → `4.1.0`) for backward-compatible feature additions.
   - Use **major** (`4.x` → `5.0.0`) for breaking changes.
2) Update release notes in the relevant docs under `docs/` (e.g. `docs/auth.md`).
3) Merge to `develop`, then push a tag `vX.Y.Z` to trigger publishing to GitHub Packages.

Notes:
- The publish workflow derives the NuGet package version from the `vX.Y.Z` tag, so tags should match `PackageVersion`.
- For manual prerelease runs, use the workflow's `workflow_dispatch` trigger.

## Testing
Integration tests require a real device and local Firebase config files.
See `BUILDING.md` for setup and test commands.

## Formatting only modified files
Format all files changed on the current branch (compared to `origin/master`):
```
dotnet format Plugin.Firebase.sln --include $(git diff --name-only origin/master...HEAD)
```

Format only staged files (not yet committed):
```
dotnet format Plugin.Firebase.sln --include $(git diff --name-only --cached)
```

## Documentation
If you change behavior or APIs, update the relevant docs in `docs/` and/or `README.md`.

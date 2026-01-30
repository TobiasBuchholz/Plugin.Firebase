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

In general, bump only the package(s) whose public surface changed. A major version bump to `Plugin.Firebase.Auth`
does **not** automatically mean unrelated packages (for example `Plugin.Firebase.Firestore`) must be re-released.

Maintainers may prefer to handle version bumps as part of the release process. If you're unsure, leave versions
unchanged and call it out explicitly in the PR.

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

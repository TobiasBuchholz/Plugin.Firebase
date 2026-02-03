# Building locally

This repo builds Firebase iOS/Android libraries and NuGet packages using .NET.

## Prerequisites

- .NET SDK (see `global.json`)
- Xcode (for iOS components)
- Android SDK (for Android components)

## Restore packages

Before building, restore NuGet packages:

```sh
dotnet restore
```

### Configure GitHub Packages feed (for fork contributors)

If you are working on a fork of this repository and want to resolve NuGet packages published from your fork, run:

```sh
# Using GitHub CLI (recommended)
./scripts/configure-github-feed.sh --gh

# Or using a personal access token
export GITHUB_PACKAGES_PAT="your_github_pat_here"
./scripts/configure-github-feed.sh
```

This script:
- Auto-detects your fork owner from the git remote URL
- Configures a GitHub Packages feed (`github-<YourUsername>`)
- Allows `dotnet restore` to resolve packages published from your fork

**Note**: Your GitHub Personal Access Token must have the `read:packages` scope. See [GitHub docs](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens#creating-a-personal-access-token-classic) for token creation.

## Build

Build the solution:

```sh
dotnet build Plugin.Firebase.sln
```

## Run tests

```sh
dotnet test tests/
```

## Troubleshooting

### NU1101: Unable to find package

If you see errors like:
```
NU1101: Unable to find package AdamE.Firebase.iOS.AppCheck [...]
```

This may occur if:
1. GitHub Packages feed is not configured (see "[Configure GitHub Packages feed](#configure-github-packages-feed-for-fork-contributors)" above)
2. Your token lacks `read:packages` scope
3. The package was not published to the feed

Verify your feed configuration:
```sh
dotnet nuget list source
```

### NuGet cache issues

If NuGet caches stale packages, clear the cache:

```sh
dotnet nuget locals all --clear
```

Then retry restore:
```sh
dotnet restore
```

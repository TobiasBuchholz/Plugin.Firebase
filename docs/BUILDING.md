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

## Publishing a local debug pack (`-local`)

To quickly publish a local debug pack (with the `-local` suffix), you can use the following script:

```sh
rm -rf output && mkdir output
VERSION=4.2.0-local

for proj in \
  src/Core/Core.csproj \
  src/Analytics/Analytics.csproj \
  src/Auth/Auth.csproj \
  src/CloudMessaging/CloudMessaging.csproj \
  src/Crashlytics/Crashlytics.csproj \
  src/Firestore/Firestore.csproj \
  src/Functions/Functions.csproj \
  src/RemoteConfig/RemoteConfig.csproj \
  src/Storage/Storage.csproj \
  src/AppCheck/AppCheck.csproj \
  src/Bundled/Bundled.csproj
do
  dotnet pack "$proj" -c Release -p:PackageVersion=$VERSION -o output
done

for pkg in output/*.nupkg; do
  dotnet nuget push "$pkg" --source github-<username> --api-key "GH_Personal_Access_Token" --skip-duplicate
done
```

Replace `<username>` with your GitHub username and `GH_Personal_Access_Token` with a personal access token that has both `read:packages` and `write:packages` scopes.

---

## Testing forked native-binding packages (`-local` / `-fork`)

For short-cycle validation of binding fixes (for example AppCheck iOS):

- Use `-local` suffix for packages produced on your machine and consumed from a local NuGet source.
- Use `-fork` suffix for packages produced in fork CI and consumed from your GitHub Packages feed.

Typical workflow:

1. Publish binding package with temporary prerelease suffix (`-local` or `-fork`).
2. Update `PackageReference` in `Plugin.Firebase` to that exact prerelease version.
3. Run restore/build and validate on device/simulator.
4. Revert temporary prerelease `PackageReference` values before preparing upstream PRs.

Important: do not commit/push temporary local/fork-only package versions to upstream-facing branches unless maintainers explicitly ask for it.

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

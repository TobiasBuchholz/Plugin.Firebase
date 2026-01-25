# Packaging to GitHub Packages (NuGet)

This repo ships multiple NuGet packages (one per feature + bundled). Packaging requires
Android/iOS workloads if you pack multi-targeted TFMs.

## Pack
Pack a single package:
```
dotnet pack src/Auth/Auth.csproj -c Release -o artifacts
```

Pack all packages:
```
dotnet pack Plugin.Firebase.sln -c Release -o artifacts
```

## Push to GitHub Packages
Create a GitHub Packages feed in your account and authenticate with a PAT or `GITHUB_TOKEN`.
```
dotnet nuget push "artifacts/*.nupkg" \
  --source "https://nuget.pkg.github.com/<OWNER>/index.json" \
  --api-key "<TOKEN>" \
  --skip-duplicate
```

Notes:
- Do **not** commit tokens or config files.
- Use `--skip-duplicate` for reruns.
- Keep package versions consistent across affected projects.

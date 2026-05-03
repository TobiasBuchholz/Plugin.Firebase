# CI/CD Guidance

This repo targets multiple TFMs (`net9.0`, `net9.0-android`, `net9.0-ios`). CI can be kept
lightweight by building `net9.0` only, and optionally adding Android/iOS builds on macOS.

## Suggested CI stages
1) **Restore + build (net9.0 only)**  
   Fast validation on Linux/Windows runners.

2) **Unit tests (net9.0)**  
   Run lightweight unit tests for core mappings and helpers.

3) **Android build (optional)**  
   Requires Android SDK and `dotnet workload install android`.

4) **iOS build (optional, macOS)**  
   Requires Xcode + iOS workload.

5) **Integration tests (manual/device)**  
   Run on real devices using local Firebase configs.

## Example GitHub Actions steps (snippet)
```
- uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '9.x'
- run: dotnet restore tests/Plugin.Firebase.UnitTests/Plugin.Firebase.UnitTests.csproj
- run: dotnet restore src/Auth/Auth.csproj
- run: dotnet build src/Auth/Auth.csproj -c Release -f net9.0 --no-restore
- run: dotnet test tests/Plugin.Firebase.UnitTests/Plugin.Firebase.UnitTests.csproj -c Release --no-restore
```

## Publishing
There is no tracked publish workflow or standalone packaging guide in this repository right now. Use the local pack/push loop in `docs/BUILDING.md` as the source of truth until a publish workflow is added.

Before publishing:
- Confirm every publishable project is included in CI and local pack validation.
- Pack every project locally with the final `PackageVersion`.
- Push packages with `--skip-duplicate` only after validating the generated `.nupkg` files.
- Confirm new packages appear on NuGet or GitHub Packages after publish.

## Repository workflows
- CI (build + unit tests): `.github/workflows/ci.yml`
- Integration emulator checks: `.github/workflows/integration-emulators.yml`

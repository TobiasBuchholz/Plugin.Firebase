# CI/CD Guidance

This repo targets multiple TFMs (`net10.0`, `net10.0-android`, `net10.0-ios`). CI can be kept
lightweight by building `net10.0` only, and optionally adding Android/iOS builds on macOS.

## Suggested CI stages
1) **Restore + build (net10.0 only)**
   Fast validation on Linux/Windows runners.

2) **Unit tests (net10.0)**
   Run lightweight unit tests for core mappings and helpers.

3) **Android build (optional)**  
   Requires Android SDK, JDK 21, and `dotnet workload restore`.

4) **iOS build (optional, macOS)**  
   Requires Xcode + iOS workload.

5) **Integration tests (manual/device)**  
   Run on real devices using local Firebase configs.

## Example GitHub Actions steps (snippet)
```
- uses: actions/setup-dotnet@v5
  with:
    dotnet-version: '10.x'
- run: dotnet restore tests/Plugin.Firebase.UnitTests/Plugin.Firebase.UnitTests.csproj
- run: dotnet restore src/Auth/Auth.csproj
- run: dotnet build src/Auth/Auth.csproj -c Release -f net10.0 --no-restore
- run: dotnet test tests/Plugin.Firebase.UnitTests/Plugin.Firebase.UnitTests.csproj -c Release --no-restore
```

## Publishing
Use `docs/packaging-github-packages.md` for packaging and push steps.

## Repository workflows
- CI (build + unit tests): `.github/workflows/ci.yml`
- Publish to GitHub Packages (manual): `.github/workflows/publish-github-packages.yml`

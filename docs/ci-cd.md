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
- run: dotnet restore Plugin.Firebase.sln
- run: dotnet build src/Auth/Auth.csproj -c Release -f net9.0
- run: dotnet test tests/Plugin.Firebase.UnitTests/Plugin.Firebase.UnitTests.csproj -c Release --no-build
```

## Publishing
Use `docs/packaging-github-packages.md` for packaging and push steps.

## Repository workflows
- CI (build + unit tests): `.github/workflows/ci.yml`
- Publish to GitHub Packages (manual): `.github/workflows/publish-github-packages.yml`

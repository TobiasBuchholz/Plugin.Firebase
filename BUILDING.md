# Building Plugin.Firebase

## Prerequisites
- .NET SDK version matching `global.json`
- Android workload (for `net9.0-android`)
- iOS workload + Xcode (for `net9.0-ios`, macOS only)

Install workloads (if needed):
```
dotnet workload install android ios
```

## Restore & build
```
dotnet restore Plugin.Firebase.sln
dotnet build Plugin.Firebase.sln -c Release
```

Note: building the full solution includes the sample and integration test apps.
Running those apps requires `GoogleService-Info.plist` and `google-services.json` files (not committed).
If you don’t have local Firebase configs, use the `net9.0` build below to validate the packages.

### Build without mobile workloads
If you want to validate core code without Android/iOS toolchains:
```
dotnet build src/Auth/Auth.csproj -c Release -f net9.0
```

## Tests (integration)
Tests live under `tests/Plugin.Firebase.IntegrationTests` and run on a real device.
You must supply your own Firebase config files (not committed):
- `GoogleService-Info.plist` (iOS)
- `google-services.json` (Android)

Run:
```
dotnet test tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj --no-build
```

## Formatting
```
dotnet format Plugin.Firebase.sln
```

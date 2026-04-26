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
These require `GoogleService-Info.plist` and `google-services.json` files and may fail
without them. If you don’t have local Firebase configs, use the `net9.0` build below.

### Build without mobile workloads
If you want to validate core code without Android/iOS toolchains:
```
dotnet build src/Auth/Auth.csproj -c Release -f net9.0
```

## Tests (integration)
Tests live under `tests/Plugin.Firebase.IntegrationTests` and run on a real device or simulator.
You must supply your own Firebase config files (not committed):
- `GoogleService-Info.plist` (iOS)
- `google-services.json` (Android)

Build the iOS test app for a simulator:
```
dotnet build tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -c Debug \
  -f net9.0-ios \
  -p:RuntimeIdentifier=iossimulator-arm64 \
  -p:EnableCodeSigning=false
```

Launch it on a specific simulator:
```
dotnet build tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -t:Run \
  -c Debug \
  -f net9.0-ios \
  -p:RuntimeIdentifier=iossimulator-arm64 \
  -p:_DeviceName=:v2:udid=<simulator-udid> \
  -p:EnableCodeSigning=false
```

Use `xcrun simctl list devices available` to find a simulator UDID. The test app uses the xUnit MAUI visual runner, so once the app launches in the simulator, run the suite from the app UI.

If you have multiple Xcode versions installed, make sure the selected Xcode matches the installed .NET iOS workload. You can either switch globally with `xcode-select --switch ...` or scope a single command with `DEVELOPER_DIR=/Applications/Xcode.app/Contents/Developer`.

## Formatting
```
dotnet format Plugin.Firebase.sln
```

For more options (e.g., formatting only modified files), see [CONTRIBUTING.md / Formatting modified files](CONTRIBUTING.md#formatting-modified-files).

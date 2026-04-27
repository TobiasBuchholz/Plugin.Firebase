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

By default the integration test app uses the identifier `plugin.firebase.integrationtests`.
You can override it per-platform via MSBuild properties or a local ignored file at `tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.props.user`:

```xml
<Project>
  <PropertyGroup>
    <IntegrationTestsAndroidApplicationId>com.example.integrationtests</IntegrationTestsAndroidApplicationId>
    <IntegrationTestsIosApplicationId>com.example.integrationtests</IntegrationTestsIosApplicationId>
    <CodesignEntitlements>Platforms\iOS\Entitlements.plist.user</CodesignEntitlements>
  </PropertyGroup>
</Project>
```

If you override the iOS application id for Firebase Auth, create the matching ignored entitlements file and set its keychain access group to `$(AppIdentifierPrefix)com.example.integrationtests`.

Make sure your Firebase app registrations and generated config files match the identifier you actually build with.

For Firebase Auth integration tests, also:
- Enable the `Email/Password` and `Anonymous` sign-in providers.
- Create `custom-claims@test.com` with password `123456` and custom claims `{ "is_awesome": true }`.
- Expect `updates_user_email` to be skipped on iOS and Android because Firebase's direct email update flow now depends on deprecated project configuration.

Build the iOS test app for a simulator:
```
dotnet build tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -c Debug \
  -f net9.0-ios \
  -p:RuntimeIdentifier=iossimulator-arm64 \
  -p:EnableCodeSigning=false
```

Build the Android test app for an emulator:
```
dotnet build tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -c Debug \
  -f net9.0-android
```

The default integration-test host now uses the DeviceRunners XHarness runner so tests can be launched from the CLI. Install the tool once:
```
dotnet tool install --global Microsoft.DotNet.XHarness.CLI \
  --add-source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json \
  --version "11.0.0-prerelease*"
```

The installed command is `xharness`. If your shell cannot find it, make sure `~/.dotnet/tools` is on your `PATH`.

Run the iOS suite on a specific simulator:
```
xharness apple test \
  --target ios-simulator-64 \
  --device <simulator-udid> \
  --timeout="00:10:00" \
  --launch-timeout=00:10:00 \
  --app tests/Plugin.Firebase.IntegrationTests/bin/Debug/net9.0-ios/iossimulator-arm64/Plugin.Firebase.IntegrationTests.app \
  --output-directory artifacts/test-results/ios
```

Run the Android suite on the currently running emulator:
```
xharness android test \
  --timeout="00:10:00" \
  --launch-timeout=00:10:00 \
  --package-name <package-id> \
  --instrumentation devicerunners.xharness.maui.XHarnessInstrumentation \
  --app tests/Plugin.Firebase.IntegrationTests/bin/Debug/net9.0-android/<package-id>-Signed.apk \
  --output-directory artifacts/test-results/android \
  --verbosity=Debug
```

Use `xcrun simctl list devices available` to find a simulator UDID and `adb devices` to verify the Android emulator is online. If you keep the default application ids, `<package-id>` is `plugin.firebase.integrationtests`. If you override the ids in `Plugin.Firebase.IntegrationTests.props.user`, use the overridden Android package id in both `--package-name` and the APK filename.

If you want the interactive visual runner instead, opt in explicitly:
- On iOS simulators, relaunch with `SIMCTL_CHILD_PLUGIN_FIREBASE_USE_VISUAL_RUNNER=1`.
- On Android emulators, run `adb shell setprop debug.pluginfirebase.visual.use 1` before launching the app.

Harness notes:
- The integration fixtures run sequentially on purpose. The suite shares backend state, emulator state, and cleanup code across tests, so disabling xUnit parallelization avoids order-dependent failures that are hard to reproduce on device runners.
- Each test writes `[TEST START]` and `[TEST END]` breadcrumbs to the runner output. If a CLI run appears hung, check the xharness log, simulator console output, or Android logcat to see which test last started.
- iOS simulator builds ad-hoc re-sign the generated app bundle and bundled .NET runtime libraries after `dotnet build`. This is a simulator-only workaround for Xcode 26 / macOS 26 code-signature validation and is not required for real-device builds.

To route Cloud Functions calls to the local emulator on an iOS simulator, start the emulator:
```
cd tests/cloud-functions
firebase emulators:start --only functions
```

For the default iOS CLI/XHarness flow, add these flags to the `xharness apple test` command:
```
--set-env=PLUGIN_FIREBASE_USE_FUNCTIONS_EMULATOR=1 \
--set-env=PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_HOST=localhost \
--set-env=PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_PORT=5001
```

If `PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_HOST` is omitted, the integration app defaults to `localhost` on iOS and `10.0.2.2` on Android. If `PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_PORT` is omitted, it defaults to `5001`.

For the default Android CLI/XHarness flow, set system properties before invoking `xharness android test`:
```
adb shell setprop debug.pluginfirebase.functions.use 1
adb shell setprop debug.pluginfirebase.functions.host 10.0.2.2
adb shell setprop debug.pluginfirebase.functions.port 5001
```

For the interactive visual runner instead:
```
SIMCTL_CHILD_PLUGIN_FIREBASE_USE_VISUAL_RUNNER=1 \
SIMCTL_CHILD_PLUGIN_FIREBASE_USE_FUNCTIONS_EMULATOR=1 \
SIMCTL_CHILD_PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_HOST=localhost \
SIMCTL_CHILD_PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_PORT=5001 \
xcrun simctl launch --terminate-running-process <simulator-udid> <bundle-id>
```

To route Firebase Storage calls to the local emulator on an iOS simulator, start the emulator:
```
cd tests/cloud-functions
firebase emulators:start --only storage
```

For the default iOS CLI/XHarness flow, add these flags to the `xharness apple test` command:
```
--set-env=PLUGIN_FIREBASE_USE_STORAGE_EMULATOR=1 \
--set-env=PLUGIN_FIREBASE_STORAGE_EMULATOR_HOST=localhost \
--set-env=PLUGIN_FIREBASE_STORAGE_EMULATOR_PORT=9199
```

If `PLUGIN_FIREBASE_STORAGE_EMULATOR_HOST` is omitted, the integration app defaults to `localhost` on iOS and `10.0.2.2` on Android. If `PLUGIN_FIREBASE_STORAGE_EMULATOR_PORT` is omitted, it defaults to `9199`.

For the default Android CLI/XHarness flow, set system properties before invoking `xharness android test`:
```
adb shell setprop debug.pluginfirebase.storage.use 1
adb shell setprop debug.pluginfirebase.storage.host 10.0.2.2
adb shell setprop debug.pluginfirebase.storage.port 9199
```

For the interactive visual runner instead:
```
SIMCTL_CHILD_PLUGIN_FIREBASE_USE_VISUAL_RUNNER=1 \
SIMCTL_CHILD_PLUGIN_FIREBASE_USE_STORAGE_EMULATOR=1 \
SIMCTL_CHILD_PLUGIN_FIREBASE_STORAGE_EMULATOR_HOST=localhost \
SIMCTL_CHILD_PLUGIN_FIREBASE_STORAGE_EMULATOR_PORT=9199 \
xcrun simctl launch --terminate-running-process <simulator-udid> <bundle-id>
```

If you have multiple Xcode versions installed, make sure the selected Xcode matches the installed .NET iOS workload. You can either switch globally with `xcode-select --switch ...` or scope a single command with `DEVELOPER_DIR=/Applications/Xcode.app/Contents/Developer`.

## Formatting
```
dotnet format Plugin.Firebase.sln
```

For more options (e.g., formatting only modified files), see [CONTRIBUTING.md / Formatting modified files](CONTRIBUTING.md#formatting-modified-files).

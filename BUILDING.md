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
The sample app still requires local Firebase config files. The integration test app
uses emulator-safe dummy Firebase options by default.

### Build without mobile workloads
If you want to validate core code without Android/iOS toolchains:
```
dotnet build src/Auth/Auth.csproj -c Release -f net9.0
```

## Tests (integration)
Tests live under `tests/Plugin.Firebase.IntegrationTests` and run on a real device or simulator.
By default they initialize Firebase with committed dummy options for the Firebase Local Emulator Suite, so no real Firebase project or config files are required for the emulator-backed suite.

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

### Emulator backend (default)

The default backend is `emulator` (`PLUGIN_FIREBASE_TEST_BACKEND=emulator`, or Android system property `debug.pluginfirebase.backend=emulator`). It covers Auth, Firestore, Functions, and Storage. Analytics, Remote Config, and App Check token tests are skipped by default because Firebase does not provide local emulators for them.

Start the emulator suite from `tests/cloud-functions`:

```
cd tests/cloud-functions/functions
npm install --legacy-peer-deps
npm run build
cd ..
firebase emulators:start --only auth,firestore,functions,storage
```

Before running the device suite against a fresh Auth emulator, seed the custom-claims account in another terminal:

```
cd tests/cloud-functions
node scripts/seed-auth-emulator.js
```

For one-shot local runs, wrap the device command with `emulators:exec`:

```
cd tests/cloud-functions
firebase emulators:exec --project demo-pluginfirebase-integrationtests --only auth,firestore,functions,storage \
  "node scripts/seed-auth-emulator.js && <xharness command>"
```

Default emulator endpoints are:

| Service | iOS host | Android host | Port |
|---|---|---|---|
| Auth | `localhost` | `10.0.2.2` | `9099` |
| Firestore | `localhost` | `10.0.2.2` | `8080` |
| Functions | `localhost` | `10.0.2.2` | `5001` |
| Storage | `localhost` | `10.0.2.2` | `9199` |

Override them with `PLUGIN_FIREBASE_<SERVICE>_EMULATOR_HOST` / `PLUGIN_FIREBASE_<SERVICE>_EMULATOR_PORT`, or Android system properties like `debug.pluginfirebase.auth.host` and `debug.pluginfirebase.auth.port`.

The Auth emulator seed script recreates `custom-claims@test.com` with password `123456` and custom claims `{ "is_awesome": true }`. The `updates_user_email` test remains skipped on iOS and Android because Firebase's direct email update flow now depends on deprecated project configuration.

The `.github/workflows/integration-emulators.yml` workflow runs the emulator-backed Android and iOS suites on pull requests and can also be started manually with `workflow_dispatch`. Branch protection should require the `integration-emulators-android` and `integration-emulators-ios` checks.

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

Use `xcrun simctl list devices available` to find a simulator UDID and `adb devices` to verify the Android emulator is online before running the Android command. XHarness uses the only connected adb target by default; if `adb devices` lists more than one device or emulator, add `--device-id <adb-device-id>` to the `xharness android test` command. If you keep the default application ids, `<package-id>` is `plugin.firebase.integrationtests`. If you override the ids in `Plugin.Firebase.IntegrationTests.props.user`, use the overridden Android package id in both `--package-name` and the APK filename.

### Real Firebase backend (opt-in)

Set `PLUGIN_FIREBASE_TEST_BACKEND=real` on iOS or `adb shell setprop debug.pluginfirebase.backend real` on Android to run against a real Firebase project. You must supply your own uncommitted config files:

- `GoogleService-Info.plist` (iOS)
- `google-services.json` (Android)

Make sure your Firebase app registrations and generated config files match the identifier you actually build with.

For real Firebase Auth integration tests:
- Enable the `Email/Password` and `Anonymous` sign-in providers.
- Create `custom-claims@test.com` with password `123456` and custom claims `{ "is_awesome": true }`.

Real-project Remote Config tests expect published values for `remote_string`, `remote_long`, `remote_double`, and `remote_bool`; see `docs/BUILDING.md` for the full setup table.

If you want the interactive visual runner instead, opt in explicitly:
- On iOS simulators, relaunch with `SIMCTL_CHILD_PLUGIN_FIREBASE_USE_VISUAL_RUNNER=1`.
- On Android emulators, run `adb shell setprop debug.pluginfirebase.visual.use 1` before launching the app.

Harness notes:
- The integration fixtures run sequentially on purpose. The suite shares backend state, emulator state, and cleanup code across tests, so disabling xUnit parallelization avoids order-dependent failures that are hard to reproduce on device runners.
- Each test writes `[TEST START]` and `[TEST END]` breadcrumbs to the runner output. If a CLI run appears hung, check the xharness log, simulator console output, or Android logcat to see which test last started.
- iOS simulator builds ad-hoc re-sign the generated app bundle and bundled .NET runtime libraries after `dotnet build`. This is a simulator-only workaround for Xcode 26 / macOS 26 code-signature validation and is not required for real-device builds.

If you have multiple Xcode versions installed, make sure the selected Xcode matches the installed .NET iOS workload. You can either switch globally with `xcode-select --switch ...` or scope a single command with `DEVELOPER_DIR=/Applications/Xcode.app/Contents/Developer`.

## Formatting
```
dotnet format Plugin.Firebase.sln
```

For more options (e.g., formatting only modified files), see [CONTRIBUTING.md / Formatting modified files](CONTRIBUTING.md#formatting-modified-files).

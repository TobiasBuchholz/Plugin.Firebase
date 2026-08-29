# Building Plugin.Firebase

## Prerequisites
- .NET SDK version matching `global.json`
- Android workload (for `net10.0-android`)
- iOS workload + Xcode (for `net10.0-ios`, macOS only)

Restore the pinned workloads (if needed):
```
dotnet workload restore Plugin.Firebase.sln
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
dotnet build src/Auth/Auth.csproj -c Release -f net10.0
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
scripts/run-integration-emulators.sh android
DEVICE_ID=<simulator-udid> scripts/run-integration-emulators.sh ios
```

The runner script calls `scripts/check-integration-environment.sh android|ios` before launching emulators. Run the preflight directly when diagnosing setup issues; it checks required CLIs, Functions build output, emulator ports, and device/simulator availability. Set `SKIP_INTEGRATION_PREFLIGHT=1` only when a CI step has already guaranteed those conditions.

Default emulator endpoints are:

| Service | iOS host | Android host | Port |
|---|---|---|---|
| Auth | `localhost` | `10.0.2.2` | `9099` |
| Firestore | `localhost` | `10.0.2.2` | `8080` |
| Functions | `localhost` | `10.0.2.2` | `5001` |
| Storage | `localhost` | `10.0.2.2` | `9199` |

For iOS simulator runs, override them with `SIMCTL_CHILD_PLUGIN_FIREBASE_<SERVICE>_EMULATOR_HOST` / `SIMCTL_CHILD_PLUGIN_FIREBASE_<SERVICE>_EMULATOR_PORT`. For Android `dotnet test`, set the corresponding `PLUGIN_FIREBASE_*` values in the host environment; the test build embeds them in the app. Existing `debug.pluginfirebase.*` system properties remain useful for direct app launches.

The Auth emulator seed script recreates `custom-claims@test.com` with password `123456` and the nested custom claims asserted by the Auth fixture. The `updates_user_email` test remains skipped on iOS and Android because Firebase's direct email update flow now depends on deprecated project configuration.

The `.github/workflows/integration-emulators.yml` workflow runs the emulator-backed Android and iOS suites on pull requests and can also be started manually with `workflow_dispatch`. Branch protection should require the `integration-emulators-android` and `integration-emulators-ios` checks. Its GitHub step summary is generated from DeviceRunners TRX output and includes totals, failed tests, skipped tests, slow tests, and recent `[TEST START]` breadcrumbs when logs are present.

The integration app references `DeviceRunners.Testing.Targets`. It extends `dotnet test` for device targets, builds and deploys the MAUI app, starts the tests in headless mode, and writes standard TRX output. The DeviceRunners CLI is bundled with the package, so no global runner tool is required. `scripts/install-integration-test-tools.sh` installs only the Firebase CLI used by the emulator wrapper.

Run the iOS suite on a specific simulator:
```
dotnet test tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -c Debug \
  -f net10.0-ios \
  -p:RuntimeIdentifier=iossimulator-arm64 \
  -p:EnableCodeSigning=false \
  -p:DeviceRunnersDevice=<simulator-udid> \
  --logger trx \
  --results-directory artifacts/test-results/ios
```

Run the Android suite on the only connected emulator or device:
```
dotnet test tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -c Debug \
  -f net10.0-android \
  --logger trx \
  --results-directory artifacts/test-results/android
```

The emulator wrapper runs those commands inside `firebase emulators:exec`, seeds Auth, and uses ten-minute DeviceRunners connection/data timeouts:

```
scripts/run-integration-emulators.sh android
DEVICE_ID=<simulator-udid> scripts/run-integration-emulators.sh ios
```

Use `xcrun simctl list devices available` to find an iOS simulator UDID and `adb devices` to verify the Android emulator is online. Pass a specific Android adb serial as `-p:DeviceRunnersDevice=<adb-serial>` for a direct command, or set `ANDROID_DEVICE_ID=<adb-serial>` for the wrapper. The iOS wrapper maps `DEVICE_ID` to the same `DeviceRunnersDevice` property. Do not use `--no-build`: on Android, DeviceRunners injects its auto-run, TCP, and `PLUGIN_FIREBASE_*` settings while `dotnet test` builds the app.

### Real Firebase backend (opt-in)

Set `SIMCTL_CHILD_PLUGIN_FIREBASE_TEST_BACKEND=real` before an iOS simulator run or `PLUGIN_FIREBASE_TEST_BACKEND=real` before Android `dotnet test` to use a real Firebase project. Other iOS app settings use the same `SIMCTL_CHILD_` prefix; Android `PLUGIN_FIREBASE_*` values are embedded during the test build. The documented `debug.pluginfirebase.*` system properties remain available for direct Android launches. You must supply your own uncommitted config files:

- `GoogleService-Info.plist` (iOS)
- `google-services.json` (Android)

Make sure your Firebase app registrations and generated config files match the identifier you actually build with.

For real Firebase Auth integration tests:
- Enable the `Email/Password` and `Anonymous` sign-in providers.
- Create `custom-claims@test.com` with password `123456` and the nested custom claims used by `tests/cloud-functions/scripts/seed-auth-emulator.js`.

Real-project Cloud Functions, Storage, and Remote Config tests expect deployed test functions, bucket seed files, and published Remote Config values; see `docs/BUILDING.md` for the full setup tables.

Launching the integration app directly from an IDE, `xcrun simctl launch`, or `adb shell am start` opens the interactive visual runner by default. `AddCliConfiguration()` switches it to headless auto-run only when DeviceRunners starts it through `dotnet test`.

Harness notes:
- The integration fixtures run sequentially on purpose. The suite shares backend state, emulator state, and cleanup code across tests, so disabling xUnit parallelization avoids order-dependent failures that are hard to reproduce on device runners.
- Each test writes `[TEST START]` and `[TEST END]` breadcrumbs to the runner output. If a run appears hung, inspect `tcp-test-events.jsonl`, `ios-device-log.txt`, or `logcat.txt` in the selected results directory to see which test last started.
- iOS simulator builds ad-hoc re-sign the generated app bundle and bundled .NET runtime libraries after `dotnet build`. This is a simulator-only workaround for Xcode 26 / macOS 26 code-signature validation and is not required for real-device builds.

If you have multiple Xcode versions installed, make sure the selected Xcode matches the installed .NET iOS workload. You can either switch globally with `xcode-select --switch ...` or scope a single command with `DEVELOPER_DIR=/Applications/Xcode.app/Contents/Developer`.

## Formatting
```
dotnet format Plugin.Firebase.sln
```

For more options (e.g., formatting only modified files), see [CONTRIBUTING.md / Formatting modified files](CONTRIBUTING.md#formatting-modified-files).

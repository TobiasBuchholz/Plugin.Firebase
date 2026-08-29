# Building locally

This repo builds Firebase iOS/Android libraries and NuGet packages using .NET.

## Prerequisites

- .NET SDK (see `global.json`)
- Xcode (for iOS components)
- Android SDK (for Android components)

## Restore workloads and packages

Before building, restore the workload set pinned by `global.json`, then restore NuGet packages:

```sh
dotnet workload restore Plugin.Firebase.sln
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
  src/Installations/Installations.csproj \
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

Unit tests:

```sh
dotnet test tests/Plugin.Firebase.UnitTests/Plugin.Firebase.UnitTests.csproj
```

The integration test project references `DeviceRunners.Testing.Targets`. It extends `dotnet test` for device targets, builds and deploys the MAUI app, starts its visual runner in headless mode, and writes standard TRX output. The DeviceRunners CLI is bundled with the package, so no global runner tool is required.

iOS integration tests run on a specific simulator:

```sh
dotnet test tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -c Debug \
  -f net10.0-ios \
  -p:RuntimeIdentifier=iossimulator-arm64 \
  -p:EnableCodeSigning=false \
  -p:DeviceRunnersDevice=<simulator-udid> \
  --logger trx \
  --results-directory artifacts/test-results/ios
```

Android integration tests run on the only connected emulator or device:

```sh
dotnet test tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -c Debug \
  -f net10.0-android \
  --logger trx \
  --results-directory artifacts/test-results/android
```

The emulator wrapper installs the Firebase CLI, seeds Auth, and runs those commands inside `firebase emulators:exec`:

```sh
scripts/install-integration-test-tools.sh
scripts/run-integration-emulators.sh android
DEVICE_ID=<simulator-udid> scripts/run-integration-emulators.sh ios
```

Use `xcrun simctl list devices available` to discover simulator UDIDs and `adb devices` to verify the Android emulator is online. Set `ANDROID_DEVICE_ID=<adb-serial>` for the wrapper when more than one Android target is connected; direct commands use `-p:DeviceRunnersDevice=<adb-serial>`. The iOS wrapper maps `DEVICE_ID` to that same property. The preflight checks required CLIs, Functions build output, emulator ports, and target availability. Set `SKIP_INTEGRATION_PREFLIGHT=1` only when another step already guarantees those conditions.

Do not pass `--no-build`: on Android, DeviceRunners injects its auto-run, TCP, and host `PLUGIN_FIREBASE_*` settings while `dotnet test` builds the app. Launching the app directly from an IDE, `xcrun simctl launch`, or `adb shell am start` opens the interactive visual runner by default; `AddCliConfiguration()` enables headless auto-run only for `dotnet test` launches.

Harness notes:

- The integration fixtures run sequentially on purpose. The suite shares backend state, emulator state, and cleanup code across tests, so disabling xUnit parallelization avoids order-dependent failures that are hard to reproduce on device runners.
- Each test writes `[TEST START]` and `[TEST END]` breadcrumbs to the runner output. If a run appears hung, inspect `tcp-test-events.jsonl`, `ios-device-log.txt`, or `logcat.txt` in the selected results directory to see which test last started. The CI summary includes recent breadcrumbs plus failed, skipped, and slow test details from TRX/log data.
- iOS simulator builds ad-hoc re-sign the generated app bundle and bundled .NET runtime libraries after `dotnet build`. This is a simulator-only workaround for Xcode 26 / macOS 26 code-signature validation and is not required for real-device builds.

## Emulator-backed integration tests (default)

The integration test app defaults to `PLUGIN_FIREBASE_TEST_BACKEND=emulator` and initializes Firebase with dummy options for project `demo-pluginfirebase-integrationtests`. No real Firebase config files are required for the default Auth, Firestore, Functions, and Storage coverage.

Install and build the local Functions project once:

```sh
cd tests/cloud-functions/functions
npm install --legacy-peer-deps
npm run build
```

Run the Firebase Local Emulator Suite from `tests/cloud-functions`:

```sh
firebase emulators:start --only auth,firestore,functions,storage
```

Seed the Auth emulator before launching the device test app:

```sh
node scripts/seed-auth-emulator.js
```

For one-shot emulator-backed runs, use the shared runner script:

```sh
scripts/run-integration-emulators.sh android
DEVICE_ID=<simulator-udid> scripts/run-integration-emulators.sh ios
```

Run `scripts/check-integration-environment.sh android|ios` directly for a fast setup check without launching emulators.

Default emulator ports are Auth `9099`, Firestore `8080`, Functions `5001`, and Storage `9199`. The app uses `localhost` on iOS and `10.0.2.2` on Android. For an iOS simulator, override app settings with `SIMCTL_CHILD_PLUGIN_FIREBASE_<SERVICE>_EMULATOR_HOST` / `SIMCTL_CHILD_PLUGIN_FIREBASE_<SERVICE>_EMULATOR_PORT`. For Android `dotnet test`, set the corresponding `PLUGIN_FIREBASE_*` host variables; direct launches can use system properties such as `debug.pluginfirebase.auth.host`.

Analytics, Remote Config, Performance Monitoring ingestion, and App Check token tests are skipped on the emulator backend because Firebase does not provide local emulators for those products. Use the real backend below when validating them.

Performance Monitoring still runs wrapper contract tests on the default emulator-backed app because custom traces and HTTP metrics can be created with dummy Firebase options. Those tests validate the local SDK calls and wrapper behavior only; automated tests do not wait for traces or metrics to appear in the Firebase Console.

The `.github/workflows/integration-emulators.yml` workflow runs the emulator-backed Android and iOS suites as PR-gated checks and still supports manual reruns with `workflow_dispatch`. Branch protection should require the `integration-emulators-android` and `integration-emulators-ios` checks. Its summary output keeps the totals table and adds failures, skips, slow tests, and recent test breadcrumbs when available.

## Real Firebase project setup for integration tests

Set `SIMCTL_CHILD_PLUGIN_FIREBASE_TEST_BACKEND=real` before an iOS simulator run or `PLUGIN_FIREBASE_TEST_BACKEND=real` before Android `dotnet test` to use a dedicated Firebase project. Other iOS app settings use the same `SIMCTL_CHILD_` prefix; Android `PLUGIN_FIREBASE_*` settings are embedded during the test build. The documented `debug.pluginfirebase.*` system properties remain available for direct launches. Below is the full real-project configuration needed.

### Firebase config files

Place your Firebase config files (not committed to the repo) in the integration test project root:

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

### Authentication

1. Enable the **Email/Password** and **Anonymous** sign-in providers.
2. Create the following user manually (or via Firebase Admin SDK):

   | Email | Password | Custom Claims |
   |---|---|---|
   | `custom-claims@test.com` | `123456` | See the claim payload below |

   Custom claims must be set via the Firebase Admin SDK or a Cloud Function — they cannot be set from the Firebase Console UI. Example using the Admin SDK:
   ```js
   admin.auth().getUserByEmail('custom-claims@test.com')
     .then(user => admin.auth().setCustomUserClaims(user.uid, {
       is_awesome: true,
       nested_object: {
         enabled: true,
         roles: ['admin', 'tester'],
         metadata: {
           source: 'emulator',
           version: 2,
         },
         history: [
           { action: 'created', count: 1 },
           { action: 'updated', count: 2 },
         ],
         score: 7,
         ratio: 1.5,
         optional: null,
       },
       nested_array: [
         { name: 'first', flags: [true, false] },
         { name: 'second', metadata: { source: 'emulator' } },
       ],
     }));
   ```

3. All other test users (`sign-in-with-pw@test.com`, `to-delete@test.com`, etc.) are created and cleaned up automatically by the test suite via `createsUserAutomatically`.
4. On iOS and Android, `updates_user_email` is intentionally skipped. Firebase's direct email update flow now depends on deprecated project configuration, so the test is not portable to newly configured projects.

### Cloud Functions

Before deploying, make sure `tests/cloud-functions/.firebaserc` targets the same Firebase project as your `GoogleService-Info.plist` / `google-services.json`. Open the file and update the `default` project if needed:

```json
{
  "projects": {
    "default": "<your-firebase-project-id>"
  }
}
```

Then install dependencies and deploy:

```sh
cd tests/cloud-functions/functions
npm install --legacy-peer-deps
cd ..
firebase deploy --only functions
```

When using the real backend, you may still route only `FunctionsFixture` to the local Functions emulator instead of deploying:

```sh
cd tests/cloud-functions
firebase emulators:start --only functions
```

For iOS `dotnet test`, pass app settings through the simulator environment:

```sh
SIMCTL_CHILD_PLUGIN_FIREBASE_TEST_BACKEND=real \
SIMCTL_CHILD_PLUGIN_FIREBASE_USE_FUNCTIONS_EMULATOR=1 \
SIMCTL_CHILD_PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_HOST=localhost \
SIMCTL_CHILD_PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_PORT=5001 \
dotnet test tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -c Debug -f net10.0-ios \
  -p:RuntimeIdentifier=iossimulator-arm64 \
  -p:DeviceRunnersDevice=<simulator-udid> \
  --logger trx
```

For Android, pass the same app settings through the host environment:

```sh
PLUGIN_FIREBASE_TEST_BACKEND=real \
PLUGIN_FIREBASE_USE_FUNCTIONS_EMULATOR=1 \
PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_HOST=10.0.2.2 \
PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_PORT=5001 \
dotnet test tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -c Debug -f net10.0-android \
  --logger trx
```

The same `SIMCTL_CHILD_*` values apply when launching the iOS app directly; a direct launch opens the interactive visual runner without another switch:

```sh
SIMCTL_CHILD_PLUGIN_FIREBASE_TEST_BACKEND=real \
SIMCTL_CHILD_PLUGIN_FIREBASE_USE_FUNCTIONS_EMULATOR=1 \
SIMCTL_CHILD_PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_HOST=localhost \
SIMCTL_CHILD_PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_PORT=5001 \
xcrun simctl launch --terminate-running-process <simulator-udid> <bundle-id>
```

Required functions:

| Function | Type | Purpose |
|---|---|---|
| `convertToLeet` | `https.onCall` | Called by `FunctionsFixture` |
| `returnObjectPayload` | `https.onCall` | Verifies callable object response deserialization |
| `returnArrayPayload` | `https.onCall` | Verifies callable array response deserialization |
| `returnStringPayload` | `https.onCall` | Verifies callable string response deserialization |
| `returnEscapedStringPayload` | `https.onCall` | Verifies callable escaped string response deserialization |
| `returnNumberPayload` | `https.onCall` | Verifies callable number response deserialization |
| `returnBooleanPayload` | `https.onCall` | Verifies callable boolean response deserialization |
| `returnNullPayload` | `https.onCall` | Verifies callable null response deserialization |
| `createCustomToken` | `https.onCall` | Emulator-only helper that mints custom tokens for Auth acceptance tests; it rejects non-emulator calls and must not be deployed as a real token-minting endpoint |
| `echoAuthContext` | `https.onCall` | Verifies callable auth context propagation |
| `throwStructuredError` | `https.onCall` | Verifies callable error propagation |
| `regionalPing` | `https.onCall`, `southamerica-east1` | Verifies configured Functions regions |
| `addMessage` | `https.onRequest` | Writes to Firestore `messages` collection |
| `makeUppercase` | `firestore.onCreate` | Triggered on `/messages/{documentId}` |
| `echo` | `https.onRequest` | Echoes request body |

### Firestore

1. Create a **Firestore database** in Native mode.
2. Set permissive security rules for testing (do not use in production):
   ```
   rules_version = '2';
   service cloud.firestore {
     match /databases/{database}/documents {
       match /{document=**} {
         allow read, write: if true;
       }
     }
   }
   ```
3. Create the following **composite indexes** on the `pokemons` collection:
   - `poke_type` ASC, `height_in_cm` ASC
   - `poke_type` ASC, `name` ASC

   The test suite seeds the `pokemons` collection automatically via `PokemonFactory.CreateBasePokemonsAtFirestoreAsync()`.

### Remote Config

Set the following parameters in **Remote Config** in the Firebase Console:

| Key | Type | Value |
|---|---|---|
| `remote_string` | String | `remote_value` |
| `remote_long` | Number | `1337` |
| `remote_double` | Number | `13.37` |
| `remote_bool` | Boolean | `true` |

Publish the Remote Config after adding the parameters.

### Storage

Use the default Storage bucket. Create the following files:

| Path | Content |
|---|---|
| `files_to_keep/text_1.txt` | Any text (tests expect 34 bytes) |
| `files_to_keep/text_2.txt` | Any text |
| `files_to_keep/text_3.txt` | Any text |

The `files_to_keep/` directory must contain exactly **3 files** (asserted by `lists_all_files`). All other storage paths (`texts/*`, `files_to_delete/*`) are created and cleaned up by the tests.

When using the real backend, you may still route only `StorageFixture` to the local Storage emulator. The repository includes permissive emulator rules in `tests/cloud-functions/storage.rules`:

```sh
cd tests/cloud-functions
firebase emulators:start --only storage
```

For iOS `dotnet test`, pass app settings through the simulator environment:

```sh
SIMCTL_CHILD_PLUGIN_FIREBASE_TEST_BACKEND=real \
SIMCTL_CHILD_PLUGIN_FIREBASE_USE_STORAGE_EMULATOR=1 \
SIMCTL_CHILD_PLUGIN_FIREBASE_STORAGE_EMULATOR_HOST=localhost \
SIMCTL_CHILD_PLUGIN_FIREBASE_STORAGE_EMULATOR_PORT=9199 \
dotnet test tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -c Debug -f net10.0-ios \
  -p:RuntimeIdentifier=iossimulator-arm64 \
  -p:DeviceRunnersDevice=<simulator-udid> \
  --logger trx
```

For Android, pass the same app settings through the host environment:

```sh
PLUGIN_FIREBASE_TEST_BACKEND=real \
PLUGIN_FIREBASE_USE_STORAGE_EMULATOR=1 \
PLUGIN_FIREBASE_STORAGE_EMULATOR_HOST=10.0.2.2 \
PLUGIN_FIREBASE_STORAGE_EMULATOR_PORT=9199 \
dotnet test tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -c Debug -f net10.0-android \
  --logger trx
```

The same `SIMCTL_CHILD_*` values apply when launching the iOS app directly; a direct launch opens the interactive visual runner without another switch:

```sh
SIMCTL_CHILD_PLUGIN_FIREBASE_TEST_BACKEND=real \
SIMCTL_CHILD_PLUGIN_FIREBASE_USE_STORAGE_EMULATOR=1 \
SIMCTL_CHILD_PLUGIN_FIREBASE_STORAGE_EMULATOR_HOST=localhost \
SIMCTL_CHILD_PLUGIN_FIREBASE_STORAGE_EMULATOR_PORT=9199 \
xcrun simctl launch --terminate-running-process <simulator-udid> <bundle-id>
```

### App Check (optional)

App Check is disabled by default in the integration tests (`AppCheckOptions.Disabled`). To run the optional App Check token test, set `PLUGIN_FIREBASE_TEST_BACKEND=real` and `PLUGIN_FIREBASE_RUN_APPCHECK_TOKEN_TESTS=1`; the test harness configures `AppCheckOptions.Debug` during app startup for that opt-in path.

### Installations (optional destructive)

The Firebase Installations delete test is skipped by default. To run it, set `PLUGIN_FIREBASE_RUN_INSTALLATIONS_DELETE_TESTS=1`. This deletes the current Firebase installation ID and may affect other Firebase services tied to that installation.

### Performance Monitoring

Performance Monitoring real-backend tests are enabled by setting `PLUGIN_FIREBASE_TEST_BACKEND=real`. They validate that the configured Firebase project accepts custom trace and HTTP metric calls, but they do not assert Firebase Console ingestion because upload timing is controlled by the native SDK.

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

### CS1705: Assembly version mismatch (iOS SDK compatibility)

If you see errors like:
```
error CS1705: Assembly 'Firebase.Core' with identity 'Firebase.Core, Version=12.5.0.4, Culture=neutral, PublicKeyToken=null' 
uses 'Microsoft.iOS, Version=26.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065' which has a higher version than 
referenced assembly 'Microsoft.iOS' with identity 'Microsoft.iOS, Version=18.4.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065'
```

This typically occurs due to a mismatch between your .NET workload version and installed Xcode:

- **Rationale**: Different .NET workload sets ship different iOS SDK pack versions. For example, workload set `10.0.100` ships iOS SDK 26.0 (compatible with Xcode 26.0.x), while earlier workloads ship older SDK versions. A native binding compiled against a newer iOS SDK cannot be used with an older one.

- **Solution**: Align your .NET workload version to match your Xcode version. First, ensure you have the corresponding .NET SDK installed:

```sh
# Install the required .NET 10 SDK version (see global.json)
# Download from https://dotnet.microsoft.com/download or use a version manager

# Update workloads to the matching version
sudo dotnet workload update --version 10.0.302.1
sudo dotnet workload restore
```

- **Verification**: Confirm the workload version matches your expectation:

```sh
dotnet workload --version
```

Ensure the workload version is compatible with your Xcode version before retrying the build.

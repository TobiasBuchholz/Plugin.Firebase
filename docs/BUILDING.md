# Building locally

This repo builds Firebase iOS/Android libraries and NuGet packages using .NET.

## Prerequisites

- .NET SDK (see `global.json`)
- Xcode (for iOS components)
- Android SDK (for Android components)

## Restore packages

Before building, restore NuGet packages:

```sh
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

iOS integration tests build for simulator:

```sh
dotnet build tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -c Debug \
  -f net9.0-ios \
  -p:RuntimeIdentifier=iossimulator-arm64 \
  -p:EnableCodeSigning=false
```

Android integration tests build for emulator:

```sh
dotnet build tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -c Debug \
  -f net9.0-android
```

The default integration-test host now uses the DeviceRunners XHarness runner so tests can be launched from the CLI. Install the tool once:

```sh
dotnet tool install --global Microsoft.DotNet.XHarness.CLI \
  --add-source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json \
  --version "11.0.0-prerelease*"
```

iOS integration tests run on a specific simulator:

```sh
dotnet xharness apple test \
  --target ios-simulator-64 \
  --device <simulator-udid> \
  --timeout="00:10:00" \
  --launch-timeout=00:10:00 \
  --app tests/Plugin.Firebase.IntegrationTests/bin/Debug/net9.0-ios/iossimulator-arm64/Plugin.Firebase.IntegrationTests.app \
  --output-directory artifacts/test-results/ios
```

Android integration tests run on the currently running emulator:

```sh
dotnet xharness android test \
  --timeout="00:10:00" \
  --launch-timeout=00:10:00 \
  --package-name <package-id> \
  --instrumentation devicerunners.xharness.maui.XHarnessInstrumentation \
  --app tests/Plugin.Firebase.IntegrationTests/bin/Debug/net9.0-android/<package-id>-Signed.apk \
  --output-directory artifacts/test-results/android \
  --verbosity=Debug
```

Use `xcrun simctl list devices available` to discover simulator UDIDs and `adb devices` to verify the Android emulator is online. If you keep the default application ids, `<package-id>` is `plugin.firebase.integrationtests`. If you override the ids in `Plugin.Firebase.IntegrationTests.props.user`, use the overridden Android package id in both `--package-name` and the APK filename.

The interactive visual runner is still available, but it is opt-in:

- On iOS simulators, relaunch with `SIMCTL_CHILD_PLUGIN_FIREBASE_USE_VISUAL_RUNNER=1`.
- On Android emulators, run `adb shell setprop debug.pluginfirebase.visual.use 1` before launching the app.

## Firebase project setup for integration tests

Integration tests (`tests/Plugin.Firebase.IntegrationTests`) run on a real device or simulator and require a dedicated Firebase project. Below is the full configuration needed.

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
   | `custom-claims@test.com` | `123456` | `{ "is_awesome": true }` |

   Custom claims must be set via the Firebase Admin SDK or a Cloud Function — they cannot be set from the Firebase Console UI. Example using the Admin SDK:
   ```js
   admin.auth().getUserByEmail('custom-claims@test.com')
     .then(user => admin.auth().setCustomUserClaims(user.uid, { is_awesome: true }));
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
npm install
cd ..
firebase deploy --only functions
```

If your Firebase project stays on the Spark plan, you can still run `FunctionsFixture` locally against the Functions emulator instead of deploying:

```sh
cd tests/cloud-functions
firebase emulators:start --only functions
```

For the default iOS CLI/XHarness flow, add these flags to the `dotnet xharness apple test` command:

```sh
--set-env=PLUGIN_FIREBASE_USE_FUNCTIONS_EMULATOR=1 \
--set-env=PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_HOST=localhost \
--set-env=PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_PORT=5001
```

For the default Android CLI/XHarness flow, set system properties before invoking `dotnet xharness android test`:
```sh
adb shell setprop debug.pluginfirebase.functions.use 1
adb shell setprop debug.pluginfirebase.functions.host 10.0.2.2
adb shell setprop debug.pluginfirebase.functions.port 5001
```

For the interactive visual runner instead:
```sh
SIMCTL_CHILD_PLUGIN_FIREBASE_USE_VISUAL_RUNNER=1 \
SIMCTL_CHILD_PLUGIN_FIREBASE_USE_FUNCTIONS_EMULATOR=1 \
SIMCTL_CHILD_PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_HOST=localhost \
SIMCTL_CHILD_PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_PORT=5001 \
xcrun simctl launch --terminate-running-process <simulator-udid> <bundle-id>
```

Required functions:

| Function | Type | Purpose |
|---|---|---|
| `convertToLeet` | `https.onCall` | Called by `FunctionsFixture` |
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

If your Firebase project does not have a provisioned default bucket, you can run `StorageFixture` locally against the Storage emulator instead. The repository includes permissive emulator rules in `tests/cloud-functions/storage.rules`:

```sh
cd tests/cloud-functions
firebase emulators:start --only storage
```

For the default iOS CLI/XHarness flow, add these flags to the `dotnet xharness apple test` command:

```sh
--set-env=PLUGIN_FIREBASE_USE_STORAGE_EMULATOR=1 \
--set-env=PLUGIN_FIREBASE_STORAGE_EMULATOR_HOST=localhost \
--set-env=PLUGIN_FIREBASE_STORAGE_EMULATOR_PORT=9199
```

For the default Android CLI/XHarness flow, set system properties before invoking `dotnet xharness android test`:
```sh
adb shell setprop debug.pluginfirebase.storage.use 1
adb shell setprop debug.pluginfirebase.storage.host 10.0.2.2
adb shell setprop debug.pluginfirebase.storage.port 9199
```

For the interactive visual runner instead:
```sh
SIMCTL_CHILD_PLUGIN_FIREBASE_USE_VISUAL_RUNNER=1 \
SIMCTL_CHILD_PLUGIN_FIREBASE_USE_STORAGE_EMULATOR=1 \
SIMCTL_CHILD_PLUGIN_FIREBASE_STORAGE_EMULATOR_HOST=localhost \
SIMCTL_CHILD_PLUGIN_FIREBASE_STORAGE_EMULATOR_PORT=9199 \
xcrun simctl launch --terminate-running-process <simulator-udid> <bundle-id>
```

### App Check (optional)

App Check is disabled by default in the integration tests (`AppCheckOptions.Disabled`). To run the optional App Check token test, set the environment variable `PLUGIN_FIREBASE_RUN_APPCHECK_TOKEN_TESTS=1` and configure `AppCheckOptions.Debug`.

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
# Install the required .NET SDK version (e.g., 9.0.306 for earlier Xcode)
# Download from https://dotnet.microsoft.com/download or use a version manager

# Update workloads to the matching version
sudo dotnet workload update --version 9.0.306
sudo dotnet workload restore
```

- **Verification**: Confirm the workload version matches your expectation:

```sh
dotnet workload --version
```

Ensure the workload version is compatible with your Xcode version before retrying the build.

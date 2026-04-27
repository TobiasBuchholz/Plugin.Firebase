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

Build the Android test app for an emulator:
```
dotnet build tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -c Debug \
  -f net9.0-android
```

Install and launch it on the currently running Android emulator:
```
dotnet build tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj \
  -t:Run \
  -c Debug \
  -f net9.0-android
```

Use `adb devices` to verify the emulator is online. The integration app also uses the xUnit MAUI visual runner on Android, so once the app launches in the emulator, run the suite from the app UI.

To route Cloud Functions calls to the local emulator on an iOS simulator, start the emulator:
```
cd tests/cloud-functions
firebase emulators:start --only functions
```

Then launch the installed app through `simctl` with child environment variables:
```
SIMCTL_CHILD_PLUGIN_FIREBASE_USE_FUNCTIONS_EMULATOR=1 \
SIMCTL_CHILD_PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_HOST=localhost \
SIMCTL_CHILD_PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_PORT=5001 \
xcrun simctl launch --terminate-running-process <simulator-udid> <bundle-id>
```

If `PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_HOST` is omitted, the integration app defaults to `localhost` on iOS and `10.0.2.2` on Android. If `PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_PORT` is omitted, it defaults to `5001`.

On Android emulators, set system properties before relaunching the app:
```
adb shell setprop debug.pluginfirebase.functions.use 1
adb shell setprop debug.pluginfirebase.functions.host 10.0.2.2
adb shell setprop debug.pluginfirebase.functions.port 5001
adb shell am force-stop <package-id>
adb shell monkey -p <package-id> -c android.intent.category.LAUNCHER 1
```

To route Firebase Storage calls to the local emulator on an iOS simulator, start the emulator:
```
cd tests/cloud-functions
firebase emulators:start --only storage
```

Then launch the installed app through `simctl` with child environment variables:
```
SIMCTL_CHILD_PLUGIN_FIREBASE_USE_STORAGE_EMULATOR=1 \
SIMCTL_CHILD_PLUGIN_FIREBASE_STORAGE_EMULATOR_HOST=localhost \
SIMCTL_CHILD_PLUGIN_FIREBASE_STORAGE_EMULATOR_PORT=9199 \
xcrun simctl launch --terminate-running-process <simulator-udid> <bundle-id>
```

If `PLUGIN_FIREBASE_STORAGE_EMULATOR_HOST` is omitted, the integration app defaults to `localhost` on iOS and `10.0.2.2` on Android. If `PLUGIN_FIREBASE_STORAGE_EMULATOR_PORT` is omitted, it defaults to `9199`.

On Android emulators, set system properties before relaunching the app:
```
adb shell setprop debug.pluginfirebase.storage.use 1
adb shell setprop debug.pluginfirebase.storage.host 10.0.2.2
adb shell setprop debug.pluginfirebase.storage.port 9199
adb shell am force-stop <package-id>
adb shell monkey -p <package-id> -c android.intent.category.LAUNCHER 1
```

If you have multiple Xcode versions installed, make sure the selected Xcode matches the installed .NET iOS workload. You can either switch globally with `xcode-select --switch ...` or scope a single command with `DEVELOPER_DIR=/Applications/Xcode.app/Contents/Developer`.

## Formatting
```
dotnet format Plugin.Firebase.sln
```

For more options (e.g., formatting only modified files), see [CONTRIBUTING.md / Formatting modified files](CONTRIBUTING.md#formatting-modified-files).

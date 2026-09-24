# App Check

Firebase App Check helps protect backend resources from abuse by ensuring requests come from your authentic app.

## Installation
### Nuget
> Install-Package Plugin.Firebase.AppCheck

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup).
- Configure App Check **before** calling `CrossFirebase.Initialize()`. The plugin uses initialization hooks internally to install the provider factory at the correct moment on each platform (before `Configure()` on iOS, after `InitializeApp()` on Android).

```c#
using Plugin.Firebase.AppCheck;

#if IOS
CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);   // registers a "before configure" hook
CrossFirebase.Initialize();                                // hook fires here
#elif ANDROID
CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);   // registers an "after initialize" hook
CrossFirebase.Initialize(activity, activityProvider);      // hook fires here
#endif
```

> **Note:** On Android, calling `Configure()` _after_ `CrossFirebase.Initialize()` also works: the provider factory is installed immediately (see [Changing the provider after initialization (Android)](#changing-the-provider-after-initialization-android)). On iOS the provider factory has to be set before `CrossFirebase.Initialize()`; a later `Configure()` call currently has no native effect ([#699](https://github.com/TobiasBuchholz/Plugin.Firebase/issues/699)). Calling it before initialization is the recommended pattern because it behaves the same on both platforms and avoids a window where Firebase is alive without a provider.

### Providers
| Provider | Platforms | Typical usage |
|---|---|---|
| `Disabled` | All | No provider installed (default) |
| `Debug` | iOS, Android | Development / CI — prints a debug token to the console |
| `DeviceCheck` | iOS | Production fallback on older devices |
| `AppAttest` | iOS (14+) | Production (preferred on iOS) |
| `PlayIntegrity` | Android | Production (required for Google Play) |

Configuring a provider that is not supported on the current platform throws a `NotSupportedException`.

### Changing the provider after initialization (Android)

On Android, `Configure()` maps to the native `FirebaseAppCheck.installAppCheckProviderFactory()`, which may be called at any time after Firebase is initialized:

- Configuring `Debug` or `PlayIntegrity` after `CrossFirebase.Initialize()` installs that provider factory right away, in place of any factory installed before. Configuring the provider that is already installed does nothing.
- The native SDK has no API to remove an installed provider factory. While one is installed on the default Firebase app, `Configure(AppCheckOptions.Disabled)` throws an `InvalidOperationException`, and the provider stays active until the app process restarts. To run without App Check, don't configure a provider in that process.
- Android can recreate the activity without restarting the process (for example after a language or font-size change), so code in `OnCreate` runs again. A `Configure(AppCheckOptions.Disabled)` there throws if an earlier activity in the same process installed a provider, even when it comes before `CrossFirebase.Initialize()`. The bundled initializer logs this and keeps the installed provider.
- Configuring `Disabled` while no provider factory is installed does not throw. Before the first `CrossFirebase.Initialize()`, it replaces a provider that was configured earlier. A factory belongs to the Firebase app it was installed on, so after that app is deleted, `Disabled` is accepted again.

```c#
CrossFirebase.Initialize(activity, activityProvider);
CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);          // installs the debug provider factory
CrossFirebaseAppCheck.Configure(AppCheckOptions.PlayIntegrity);  // replaces it with Play Integrity
CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled);       // throws InvalidOperationException
```

### iOS — `Cannot instantiate FIRAppCheck` log message

When App Check is configured with `AppCheckOptions.Disabled`, the plugin explicitly clears the native provider factory by calling `SetAppCheckProviderFactory(null)` before `FirebaseApp.Configure()`. The native Firebase iOS SDK then logs:

```
[FirebaseAppCheck][I-FAA002001] Cannot instantiate `FIRAppCheck` for app: __FIRAPP_DEFAULT
without a provider factory. Please register a provider factory using
`AppCheck.setAppCheckProviderFactory(_ ,forAppName:)` method.
```

**This is expected and harmless.** It confirms that App Check is properly disabled — no provider factory is installed, so no App Check tokens are generated or attached to requests. Without this explicit clearing, the native SDK may auto-register a default `DeviceCheckProviderFactory` when the `FirebaseAppCheck.framework` is linked, which would produce invalid placeholder tokens on simulators and cause `CrossPlatformFirebaseAuthException: The supplied auth credential is malformed or has expired` errors on Auth and Functions requests.

### iOS enablement
App Check for iOS uses `AdamE.Firebase.iOS.AppCheck`.
Make sure your NuGet sources include the feed where that package is published (see [GoogleApisForiOSComponents](https://github.com/AdamEssenmacher/GoogleApisForiOSComponents)).

### iOS App Attest: capability + entitlements (and sandbox gotcha)

If you use `AppCheckOptions.AppAttest`, iOS requires:

1. Apple Developer capability enabled for your Bundle ID:
   - Enable **App Attest** for the app identifier, then regenerate the provisioning profile.
   - If you see two checkboxes (App Attest / App Attest Opt-In), pick **App Attest** for the normal Firebase App Check flow.
2. Entitlements include the App Attest environment:
   - `com.apple.developer.devicecheck.appattest-environment` = `production` (for TestFlight / App Store builds)
   - For DEV testing with a Development provisioning profile you may need `development`.

Firebase note (important during beta / rollout phases):
- Some App Check configurations only accept tokens generated in the **production** App Attest environment, and will reject sandbox tokens with `403 PERMISSION_DENIED / App attestation failed`.
  - Firebase doc excerpt: "App Check does not currently accept tokens generated in the App Attest sandbox environment."

Playground note:
- Ensure `GoogleService-Info.plist` matches the Playground Bundle ID and the Firebase project where App Check is configured. A mismatch commonly looks like 403 errors (or Firebase configure errors if the plist is missing).

### Android — native library packaging

.NET Android apps embed native `.so` files in the APK/AAB. If these files are compressed during packaging, the monodroid runtime crashes at startup with:

```
F monodroid: ALL entries in APK named `lib/arm64-v8a/` MUST be STORED.
```

This is **not** an App Check bug — it can happen whenever a new NuGet adds native interop libraries (like the Firebase AppCheck SDK). The fix is a single MSBuild property in your `.csproj`:

```xml
<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0-android'">
  <!-- Prevent compression of .so / .dll inside the APK/AAB -->
  <AndroidStoreUncompressedFileExtensions>so;dll</AndroidStoreUncompressedFileExtensions>
</PropertyGroup>
```

This is the standard .NET Android MSBuild property. It works with both APK and AAB formats and is compatible with Google Play requirements.

> **Do NOT use** `AndroidPackageFormat=apk` or `EmbedAssembliesIntoApk=true` as a workaround for this issue — those properties serve different purposes (debug speed, distribution format) and are not needed to fix native library compression.

See also the [sample Playground project](../sample/Playground/Playground.csproj) for a working reference.

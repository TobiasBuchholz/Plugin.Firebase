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

> **Note:** On Android, calling `Configure()` _after_ `CrossFirebase.Initialize()` also works: the provider factory is installed immediately (see [Changing the provider after initialization (Android)](#changing-the-provider-after-initialization-android)). On iOS the provider factory has to be set before `CrossFirebase.Initialize()`; afterwards, `Configure()` with any other provider throws `InvalidOperationException` (see [Configuring after initialization (iOS)](#configuring-after-initialization-ios)). Calling it before initialization is the recommended pattern because it behaves the same on both platforms and avoids a window where Firebase is alive without a provider.

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

- Configuring `Debug` or `PlayIntegrity` after `CrossFirebase.Initialize()` installs that provider factory right away, in place of any factory installed before. Configuring the provider that is already installed does nothing. The plugin only tracks factories it installed itself, so it doesn't see one installed directly through the native binding.
- The native SDK has no API to remove an installed provider factory. While one is installed on the default Firebase app, `Configure(AppCheckOptions.Disabled)` throws an `InvalidOperationException`, and the provider stays active until the app process restarts. To run without App Check, don't configure a provider in that process.
- Android can recreate the activity without restarting the process (for example after a language or font-size change), so code in `OnCreate` runs again. A `Configure(AppCheckOptions.Disabled)` there throws if an earlier activity in the same process installed a provider, even when it comes before `CrossFirebase.Initialize()`. The bundled initializer logs this and keeps the installed provider.
- Configuring `Disabled` while no provider factory is installed does not throw. Before the first `CrossFirebase.Initialize()`, it replaces a provider that was configured earlier. A factory belongs to the Firebase app it was installed on, so after that app is deleted, `Disabled` is accepted again.

```c#
CrossFirebase.Initialize(activity, activityProvider);
CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);          // installs the debug provider factory
CrossFirebaseAppCheck.Configure(AppCheckOptions.PlayIntegrity);  // replaces it with Play Integrity
CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled);       // throws InvalidOperationException
```

### Configuring after initialization (iOS)

On iOS, `Configure()` maps to the native `AppCheck.setAppCheckProviderFactory()`. The native SDK reads the factory only when Firebase configures an app, so it has to be set before `CrossFirebase.Initialize()` ([#699](https://github.com/TobiasBuchholz/Plugin.Firebase/issues/699)):

- Once the default Firebase app is configured, configuring the provider already in effect does nothing, and any other provider throws an `InvalidOperationException`. This also applies when the app was configured through the native `Firebase.Core.App.Configure()` instead of `CrossFirebase.Initialize()`.
- The provider in effect is the one the plugin set before the default app was configured. The plugin only tracks factories it set itself, so it doesn't see one set directly through the native binding, and when it set none, it treats `Disabled` as the provider in effect.
- The bundled initializer logs this exception instead of throwing, for example when it runs again to configure a secondary app, and keeps the provider in effect.
- If initialization failed before the default app was configured, or the default app was deleted, `Configure()` can change the provider again, and the next `CrossFirebase.Initialize()` installs it.
- To find out whether the default app exists, the plugin calls FirebaseCore's internal `+[FIRApp isDefaultAppConfigured]`, the check Firebase's own SDKs use, which logs nothing. Only if a FirebaseCore version lacks that method does it fall back to `FirebaseApp.app()`. That call logs `[FirebaseCore][I-COR000003] The default Firebase app has not yet been configured` when `Configure()` runs before `CrossFirebase.Initialize()`, which is harmless.

```c#
CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);       // installed by the next CrossFirebase.Initialize()
CrossFirebase.Initialize();
CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);       // does nothing
CrossFirebaseAppCheck.Configure(AppCheckOptions.AppAttest);   // throws InvalidOperationException
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

## Next-release migration notes

On Android, `CrossFirebaseAppCheck.Configure` now reaches the native SDK after initialization, and fails explicitly where the native SDK can't do what was asked:

- Configuring `Debug` or `PlayIntegrity` after `CrossFirebase.Initialize()` installs that provider factory. Previously only the first provider was installed and later changes were silently ignored.
- Configuring `Disabled` while a provider factory is installed throws `InvalidOperationException`. Previously the call succeeded but the provider stayed active. The bundled initializer logs this and keeps the installed provider.
- `DeviceCheck` and `AppAttest` throw `NotSupportedException` from `Configure` itself. Previously, when configured before initialization, the exception came from `CrossFirebase.Initialize()`. The bundled initializer now logs it and leaves App Check as it was.
- `CrossFirebaseAppCheck.Dispose()` no longer cancels a provider configured before initialization. Configure `Disabled` instead.
- Configuring the provider that is already installed does nothing.

On iOS, `CrossFirebaseAppCheck.Configure` now fails explicitly instead of being silently ignored when it can't take effect:

- Once the default Firebase app is configured, configuring any provider other than the one in effect throws `InvalidOperationException`, and configuring the provider in effect does nothing. Previously every call succeeded, but the provider was never installed. Apps that configure App Check after `CrossFirebase.Initialize()` used to start and now throw; move the call before `CrossFirebase.Initialize()`. The bundled initializer logs the exception and keeps the provider in effect.
- `PlayIntegrity` throws `NotSupportedException` from `Configure` itself. Previously, when configured before initialization, the exception came from `CrossFirebase.Initialize()` before Firebase was configured; after initialization, the call was silently ignored. The bundled initializer now logs it and leaves App Check as it was.

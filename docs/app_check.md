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

> **Note:** Calling `Configure()` _after_ `CrossFirebase.Initialize()` also works — the hook detects that initialization already happened and fires immediately. But calling it before is the recommended pattern because it is consistent across platforms and avoids a window where Firebase is alive without a provider.

### Providers
| Provider | Platforms | Typical usage |
|---|---|---|
| `Disabled` | All | No provider installed (default) |
| `Debug` | iOS, Android | Development / CI — prints a debug token to the console |
| `DeviceCheck` | iOS | Production fallback on older devices |
| `AppAttest` | iOS (14+) | Production (preferred on iOS) |
| `PlayIntegrity` | Android | Production (required for Google Play) |

Configuring a provider that is not supported on the current platform throws a `NotSupportedException`.

### iOS enablement
App Check for iOS uses `AdamE.Firebase.iOS.AppCheck`.
Make sure your NuGet sources include the feed where that package is published (see [GoogleApisForiOSComponents](https://github.com/AdamEssenmacher/GoogleApisForiOSComponents)).

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

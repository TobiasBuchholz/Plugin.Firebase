# All in one (deprecated)

> **The bundled `Plugin.Firebase` package is deprecated and will not receive further versions.** Its last release is
> 4.2.1. See [#733](https://github.com/TobiasBuchholz/Plugin.Firebase/issues/733) for the reasoning. Existing versions
> keep working and stay on NuGet, but new features and fixes only ship in the `Plugin.Firebase.*` component packages.

The bundled package installed every feature at once, for people who used the plugin before the features were split into
separate packages. It also linked every native Firebase SDK, even the ones an app never used, and on iOS linking an SDK
is enough to activate it. That is why its settings could not reliably switch features off, and why App Check and
Analytics behaved differently from what `CrossFirebaseSettings` promised.

## Migrating

Reference the packages you actually use, then initialize Firebase with `Plugin.Firebase.Core` and call the per-feature
setup you need.

**Before**

```c#
using Plugin.Firebase.Bundled.Shared;
#if IOS
using Plugin.Firebase.Bundled.Platforms.iOS;
#elif ANDROID
using Plugin.Firebase.Bundled.Platforms.Android;
#endif

var settings = new CrossFirebaseSettings(
    isAnalyticsEnabled: true,
    isAuthEnabled: true,
    isCloudMessagingEnabled: true,
    isCrashlyticsEnabled: true);

#if IOS
CrossFirebase.Initialize(settings);
#elif ANDROID
CrossFirebase.Initialize(activity, () => Platform.CurrentActivity, settings);
#endif
```

**After**

```c#
using Plugin.Firebase.Analytics;
using Plugin.Firebase.AppCheck;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Crashlytics;
#if IOS
using Plugin.Firebase.Core.Platforms.iOS;
#elif ANDROID
using Plugin.Firebase.Core.Platforms.Android;
#endif

// App Check must be configured before Firebase is initialized.
CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);

#if IOS
CrossFirebase.Initialize();
FirebaseCloudMessagingImplementation.Initialize();
CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(true);
#elif ANDROID
CrossFirebase.Initialize(activity, () => Platform.CurrentActivity);
// Like the bundled initializer, skip these when there is no default app (no matching google-services.json).
// `out var _` is always a discard; inside OnCreate((activity, _) => ...) a bare `out _` would bind to the Bundle.
if(CrossFirebase.TryGetDefaultApp(out var _)) {
    FirebaseAnalyticsImplementation.Initialize(activity);
    CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(true);
}
#endif
```

## Settings and their replacements

| `CrossFirebaseSettings` | Replacement |
|---|---|
| `appCheckOptions` | `CrossFirebaseAppCheck.Configure(options)` **before** `CrossFirebase.Initialize(...)`. See [app_check.md](app_check.md). |
| `isAnalyticsEnabled` | Android: `FirebaseAnalyticsImplementation.Initialize(activity)` after initialization. Use `CrossFirebaseAnalytics.Current.IsAnalyticsCollectionEnabled` to control collection. iOS needs no initialization. See [analytics.md](analytics.md). |
| `isCloudMessagingEnabled` | iOS: `FirebaseCloudMessagingImplementation.Initialize()` after initialization. Android needs the manifest and intent setup in [cloud_messaging.md](cloud_messaging.md), which the bundled package never did for you. |
| `isCrashlyticsEnabled` | `CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(value)` after initialization. **Read the note below.** |
| `IsPerformanceMonitoringEnabled` | `CrossFirebasePerformanceMonitoring.Current.IsDataCollectionEnabled`. See [performance_monitoring.md](performance_monitoring.md). Never shipped in a bundled release. |
| `isAuthEnabled`, `isFirestoreEnabled`, `isFunctionsEnabled`, `isRemoteConfigEnabled`, `isStorageEnabled`, `isDynamicLinksEnabled`, `IsInstallationsEnabled`, `googleRequestIdToken` | None. These flags were never read: referencing the package was what enabled the feature. |

## Things to check after migrating

- **Crashlytics collection is remembered.** `isCrashlyticsEnabled` defaulted to `false`, and the bundled initializer
  applied it on every launch. Firebase persists that choice, so an app that never passed the flag has crash reporting
  switched off on existing installs. Call `SetCrashlyticsCollectionEnabled(true)` explicitly to turn it back on.
- **App Check on iOS.** Referencing `Plugin.Firebase.AppCheck` links the native SDK, which defaults to the DeviceCheck
  provider. If you don't want App Check, call `CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled)` before
  initialization, or drop the package reference.
- **Android without a default app.** The bundled initializer skipped Analytics and Crashlytics setup when no default
  Firebase app existed, usually because of a missing or mismatched `google-services.json`. If your app can start
  without one, keep the `TryGetDefaultApp` check from the snippet above. Without it, those calls throw instead.
- **Staying on 4.1.0–4.2.1 for now?** Those versions carry the App Check
  ([#698](https://github.com/TobiasBuchholz/Plugin.Firebase/issues/698)) and Analytics
  ([#701](https://github.com/TobiasBuchholz/Plugin.Firebase/issues/701)) behavior described above.

## Release notes
- Deprecated: no further versions; 4.2.1 is the last release.
- Version 4.2.1
  - Plugin.Firebase.Auth 5.0.1
- Version 4.2.0
  - Plugin.Firebase.Auth 5.0.0
  - Plugin.Firebase.Core 4.2.0
- Version 4.1.0
  - Plugin.Firebase.Auth 4.0.1
  - Plugin.Firebase.AppCheck 4.0.0
  - Plugin.Firebase.CloudMessaging 4.0.1
  - Plugin.Firebase.Core 4.1.0
- Version 4.0.0
  - Upgrade baseline to **.NET 9+**.
- Version 3.1.4
  - Plugin.Firebase.Analytics 3.1.3
  - Plugin.Firebase.Firestore 3.1.3
- Version 3.1.3
  - Using AdamE.Firebase.iOS.* minimum version 11
- Version 3.1.2
  - Plugin.Firebase.Firestore 3.1.1
- Version 3.1.1
  - Plugin.Firebase.CloudMessaging 3.1.1
- Version 3.1.0
  - Update to .net8
- Version 3.0.0
  - Swapped Xamarin.Firebase.iOS.* packages (native SDK 8.10.0) for AdamE.Firebase.iOS.* packages (native SDK 10.24.0)
- Version 2.0.14
  - Plugin.Firebase.Functions 2.0.3
- Version 2.0.13
  - Plugin.Firebase.Firestore 2.0.7
  - Plugin.Firebase.Storage 2.0.3
- Version 2.0.12
  - Plugin.Firebase.Crashlytics 2.0.3
- Version 2.0.11
  - Plugin.Firebase.Auth 2.0.7
  - Plugin.Firebase.Firestore 2.0.6
- Version 2.0.10
  - Plugin.Firebase.Auth 2.0.6
  - Plugin.Firebase.Crashlytics 2.0.2
- Version 2.0.9
  - Plugin.Firebase.Auth 2.0.5
- Version 2.0.8
  - Plugin.Firebase.Analytics 2.0.2
  - Plugin.Firebase.CloudMessaging 2.0.4
- Version 2.0.7
  - Plugin.Firebase.Firestore 2.0.5
  - Plugin.Firebase.Functions 2.0.2
  - Plugin.Firebase.Storage 2.0.2
- Version 2.0.6
  - Plugin.Firebase.Auth 2.0.4 (Plugin.Firebase.Auth.Google 2.0.0)
  - Plugin.Firebase.Firestore 2.0.4
- Version 2.0.5
  - Plugin.Firebase.CloudMessaging 2.0.3
- Version 2.0.4
  - Plugin.Firebase.Auth 2.0.3
  - Plugin.Firebase.Firestore 2.0.3
- Version 2.0.3
  - Plugin.Firebase.Auth 2.0.2
- Version 2.0.2
  - Plugin.Firebase.CloudMessaging 2.0.2
  - Plugin.Firebase.Firestore 2.0.2
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net6.0 tfm

# Analytics

Firebase Analytics collects usage and behavior data for your app. The SDK logs two primary types of information:

- Events: What is happening in your app, such as user actions, system events, or errors
- User properties: Attributes you define to describe segments of your userbase, such as language preference or geographic location

## Installation
### NuGet
[![NuGet](https://img.shields.io/nuget/v/Plugin.Firebase.Analytics.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Analytics/)

> Install-Package Plugin.Firebase.Analytics

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/README.md#basic-setup)
- When using the standalone `Plugin.Firebase.Analytics` package on Android, initialize Analytics after calling `CrossFirebase.Initialize(...)`:

```c#
using Microsoft.Maui.ApplicationModel;
using Plugin.Firebase.Analytics;

#if IOS
using Plugin.Firebase.Core.Platforms.iOS;
#elif ANDROID
using Plugin.Firebase.Core.Platforms.Android;
#endif

builder.ConfigureLifecycleEvents(events => {
#if IOS
    events.AddiOS(iOS => iOS.WillFinishLaunching((_,__) => {
        CrossFirebase.Initialize();
        return false;
    }));
#elif ANDROID
    events.AddAndroid(android => android.OnCreate((activity, _) => {
        CrossFirebase.Initialize(activity, () => Platform.CurrentActivity);
        FirebaseAnalyticsImplementation.Initialize(activity);
    }));
#endif
});
```

The `FirebaseAnalyticsImplementation.Initialize(activity)` call is only required on Android when using the standalone Analytics package. If you use the bundled `Plugin.Firebase` package, enable Analytics through `CrossFirebaseSettings` instead:

```c#
#if ANDROID
using Plugin.Firebase.Bundled.Shared;

var settings = new CrossFirebaseSettings(isAnalyticsEnabled: true);
#endif
```

## Usage

Set Analytics consent state with the Firebase consent types exposed by the plugin:

```c#
CrossFirebaseAnalytics.Current.SetConsent(new Dictionary<ConsentType, ConsentStatus> {
    { ConsentType.AnalyticsStorage, ConsentStatus.Granted },
    { ConsentType.AdStorage, ConsentStatus.Denied },
    { ConsentType.AdUserData, ConsentStatus.Granted },
    { ConsentType.AdPersonalization, ConsentStatus.Denied }
});
```

Omitting a consent type retains its previous status. Obtain and interpret user consent in your app before passing the resulting Firebase consent values to the plugin.

Take a look at the [documentation](https://github.com/AdamEssenmacher/GoogleApisForiOSComponents/blob/master/docs/Firebase/Analytics/GettingStarted.md) for the AdamE.Firebase.iOS.Analytics packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseAnalytics.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/src/Analytics/Shared/IFirebaseAnalytics.cs)
- [tests/.../AnalyticsFixture.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/tests/Plugin.Firebase.IntegrationTests/Analytics/AnalyticsFixture.cs)

### Default event parameters

Default event parameters are included with every subsequent event. Event-specific parameters take precedence when they use the
same name.

```c#
var analytics = CrossFirebaseAnalytics.Current;

analytics.SetDefaultEventParameters(
    ("app_theme", "dark"),
    ("screen_depth", 3L),
    ("cart_value", 13.37));

analytics.LogEvent("screen_view");

analytics.SetDefaultEventParameters((IDictionary<string, object>) null);
```

Passing a typed null to the dictionary overload clears all default event parameters.

## Release notes

- Next
  - Target .NET 10 and raise the minimum Firebase iOS binding version to 12.7; the minimum platform versions remain iOS 15 and Android 23.
  - Add `SetConsent(...)` with Firebase consent types and statuses.
  - Add `SetDefaultEventParameters(...)`, including clearing defaults by passing a typed `null`.
  - Guard standalone Android use before initialization with an actionable `InvalidOperationException`.
  - Correct nullable contracts for app instance IDs, event parameters, user IDs, and user-property values.
- Version 3.1.2
  - Add collections support for Analytics (PR #432)
- Version 3.1.1
  - Using AdamE.Firebase.iOS.* minimum version 11
- Version 3.1.0
  - Update to .net8
- Version 3.0.0
  - Swapped Xamarin.Firebase.iOS.Analytics (native SDK 8.10.0) for AdamE.Firebase.iOS.Analytics (native SDK 10.24.0)
- Version 2.0.2
  - Update Xamarin.Firebase.Analytics to fix issue #172
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net6.0 tfm

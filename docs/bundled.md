# All in one
This package bundles all features into a single nuget package for people who were using prior versions of the plugin before the features were separated into single packages.  

## Installation
### NuGet
[![NuGet](https://img.shields.io/nuget/v/plugin.firebase.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase/)

> Install-Package Plugin.Firebase

#### Visual Studio 2022 on Windows:
If you encounter a build error, try to add the package via `dotnet add package Plugin.Firebase`, see [issue #69](https://github.com/TobiasBuchholz/Plugin.Firebase/issues/65) for more information.

## Setup
- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- At `Platforms/Android/Resources/values` add the following line to your `strings.xml`:
```
<resources>
    ...
    <string name="com.google.firebase.crashlytics.mapping_file_id">none</string>
    ...
</resources>
```
- For Crashlytics troubleshooting, including Android resource setup and iOS `__mh_execute_header` setup, see the [Crashlytics documentation](crashlytics.md).
- Add the following line of code to the place where your app gets bootstrapped:
```c#
using Microsoft.Maui.ApplicationModel;
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
    isDynamicLinksEnabled: true,
    isFirestoreEnabled: true,
    isFunctionsEnabled: true,
    isRemoteConfigEnabled: true,
    isStorageEnabled: true,
    googleRequestIdToken: "YOUR_GOOGLE_WEB_CLIENT_ID.apps.googleusercontent.com") {
    IsInstallationsEnabled = true,
    IsPerformanceMonitoringEnabled = true
};

#if IOS
CrossFirebase.Initialize(settings);
#elif ANDROID
CrossFirebase.Initialize(activity, () => Platform.CurrentActivity, settings);
#endif
```

When `isAnalyticsEnabled` is `true`, the bundled package initializes Firebase Analytics automatically. Do not call `FirebaseAnalyticsImplementation.Initialize(activity)` separately when using this bundled initializer.

### Performance Monitoring collection
The Firebase Performance Monitoring SDK can automatically collect app start, screen rendering, and network data when it is linked into an app. `CrossFirebaseSettings.IsPerformanceMonitoringEnabled` controls runtime collection for the bundled package, but privacy-sensitive apps should also use Firebase's documented Android manifest and iOS plist disable keys before initialization if collection must be disabled before app startup.

## Release notes
- Version 4.3.0
  - Include `Plugin.Firebase.Installations` and `Plugin.Firebase.PerformanceMonitoring` in the bundled package.
  - Add bundled settings for Installations and Performance Monitoring collection.
  - Update bundled package references to the vNext package set.
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

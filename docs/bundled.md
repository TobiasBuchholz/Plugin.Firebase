# All in one
This package bundles all features into a single nuget package for people who were using prior versions of the plugin before the features were separated into single packages.  

## Installation
### Nuget
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
- Add the following line of code to the place where your app gets bootstrapped:
```c#
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
        googleRequestIdToken: "537235599720-723cgj10dtm47b4ilvuodtp206g0q0fg.apps.googleusercontent.com")

#if IOS
  CrossFirebase.Initialize(settings);
#elif ANDROID
  CrossFirebase.Initialize(activity, settings);
#endif
```
## Release notes
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

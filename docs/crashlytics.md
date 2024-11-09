# Crashlytics

Firebase [Firebase Crashlytics](https://firebase.google.com/docs/crashlytics) is a lightweight, realtime crash reporter that helps you track, prioritize, and fix stability issues that erode your app quality. Crashlytics saves you troubleshooting time by intelligently grouping crashes and highlighting the circumstances that lead up to them.

## Installation
### Nuget
[![NuGet](https://img.shields.io/nuget/v/plugin.firebase.crashlytics.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Crashlytics/)

> Install-Package Plugin.Firebase.Crashlytics

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Add the following line of code after calling `CrossFirebase.Initialize()`:
```c#
  CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(true);
```

### iOS specifics
- Make sure to start your app without the debugger attached to be able to see the crashes in the firebase console
- For more specific instructions take a look at the official [Firebase documentation](https://firebase.google.com/docs/crashlytics/get-started?platform=ios)

### Android specifics

- At `Platforms/Android/Resources/values` add the following line to your `strings.xml`:
```
<resources>
    ...
    <string name="com.google.firebase.crashlytics.mapping_file_id">none</string>
    ...
</resources>
```
- For more specific instructions take a look at the official [Firebase documentation](https://firebase.google.com/docs/crashlytics/get-started?platform=android)

## Usage

To test if everything is setup correctly, restart the app after a forced crash and visit the [Crashlytics Dashboard](https://console.firebase.google.com/u/0/project/_/crashlytics) to view your reports and statistics.

Take a look at the [documentation](https://github.com/AdamEssenmacher/GoogleApisForiOSComponents/blob/master/docs/Firebase/Crashlytics/GettingStarted.md) for the AdamE.Firebase.iOS.Crashlytics packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following class:
- [src/.../IFirebaseCrashlytics.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Crashlytics/IFirebaseCrashlytics.cs)

## Release notes
- Version 3.1.0
  - Update to .net8
- Version 3.0.0
  - Swapped Xamarin.Firebase.iOS.Crashlytics (native SDK 8.10.0) for AdamE.Firebase.iOS.Crashlytics (native SDK 10.24.0)
- Version 2.0.3
  - Fix StackTraceParser for Crashlytics (PR #255)
- Version 2.0.2
  - Fix StackTraceParser for Crashlytics (PR #245)
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net6.0 tfm

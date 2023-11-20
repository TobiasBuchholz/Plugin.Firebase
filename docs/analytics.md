# Analyticss

Firebase Analytics collects usage and behavior data for your app. The SDK logs two primary types of information:

- Events: What is happening in your app, such as user actions, system events, or errors
- User properties: Attributes you define to describe segments of your userbase, such as language preference or geographic location

## Installation
### Nuget
[![NuGet](https://img.shields.io/nuget/v/plugin.firebase.analytics.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Analytics/)

> Install-Package Plugin.Firebase.Analytics

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Add the following lines of code after calling `CrossFirebase.Initialize()`:

```c#
#if ANDROID
  FirebaseAnalyticsImplementation.Initialize(activity);
#endif
```

## Usage

Take a look at the [documentation](https://github.com/xamarin/GoogleApisForiOSComponents/blob/master/docs/Firebase/Analytics/GettingStarted.md) for the Xamarin.Firebase.iOS.Analytics packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseAnalytics.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Analytics/IFirebaseAnalytics.cs)
- [tests/.../AnalyticsFixture.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/Plugin.Firebase.IntegrationTests/Analytics/AnalyticsFixture.cs)

## Release notes
- Version 2.0.2
  - Update Xamarin.Firebase.Analytics to fix issue #172
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net6.0 tfm

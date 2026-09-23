# Remote Config

You can use Firebase Remote Config to define parameters in your app and update their values in the cloud, allowing you to modify the appearance and behavior of your app without distributing an app update.

## Installation

### NuGet

[![NuGet](https://img.shields.io/nuget/v/Plugin.Firebase.RemoteConfig.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.RemoteConfig/)

> Install-Package Plugin.Firebase.RemoteConfig

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/README.md#basic-setup)
- Add a Remote Config key-value pair at your project in the [Firebase Console](https://console.firebase.google.com/)

## Usage

Take a look at the [documentation](https://github.com/AdamEssenmacher/GoogleApisForiOSComponents/blob/master/docs/Firebase/RemoteConfig/GettingStarted.md) for the AdamE.Firebase.iOS.RemoteConfig packages, because Plugin.Firebase's code is abstracted but still very similar.

Register a real-time update listener when you want Firebase to notify the app that published Remote Config values changed. The listener returns the changed keys; call `ActivateAsync()` explicitly when your app is ready to apply fetched values.

```csharp
var registration = CrossFirebaseRemoteConfig.Current.AddOnConfigUpdateListener(
    update => {
        if(update.UpdatedKeys.Contains("some_remote_config_key")) {
            _ = CrossFirebaseRemoteConfig.Current.ActivateAsync();
        }
    },
    error => Console.WriteLine(error));

registration.Dispose();
```

Since code should be documenting itself you can also take a look at the following classes:

- [IFirebaseRemoteConfig.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/src/RemoteConfig/Shared/RemoteConfig/IFirebaseRemoteConfig.cs)
- [Remote Config integration tests](https://github.com/TobiasBuchholz/Plugin.Firebase/tree/development/tests/Plugin.Firebase.IntegrationTests/RemoteConfig)

## Release notes

- Next
  - Target .NET 10 and raise the minimum Firebase iOS binding version to 12.7; the minimum platform versions remain iOS 15 and Android 23.
  - Corrected nullable contracts and native dictionary conversions.
  - Added real-time Remote Config update listeners; retain and dispose the returned registration to remove a listener.
- Version 3.1.1
  - Using AdamE.Firebase.iOS.* minimum version 11
- Version 3.1.0
  - Update to .net8
- Version 3.0.0
  - Swapped Xamarin.Firebase.iOS.RemoteConfig (native SDK 8.10.0) for AdamE.Firebase.iOS.RemoteConfig (native SDK 10.24.0)
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net6.0 tfm

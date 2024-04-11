# Remote Config

You can use Firebase Remote Config to define parameters in your app and update their values in the cloud, allowing you to modify the appearance and behavior of your app without distributing an app update.

## Installation
### Nuget
[![NuGet](https://img.shields.io/nuget/v/plugin.firebase.remote_config.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.RemoteConfig/)

> Install-Package Plugin.Firebase.RemoteConfig

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Add a Remote Config key-value pair at your project in the [Firebase Console](https://console.firebase.google.com/)

## Usage

Take a look at the [documentation](https://github.com/xamarin/GoogleApisForiOSComponents/blob/master/docs/Firebase/RemoteConfig/GettingStarted.md) for the Xamarin.Firebase.iOS.RemoteConfig packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseRemoteConfig.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/RemoteConfig/IFirebaseRemoteConfig.cs)
- [tests/.../RemoteConfigFixture.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/Plugin.Firebase.IntegrationTests/RemoteConfig/RemoteConfigFixture.cs)

## Release notes
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net8.0 tfm

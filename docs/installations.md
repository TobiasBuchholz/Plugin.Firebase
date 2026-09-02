# Installations

Firebase Installations provides a stable app installation identifier, auth tokens for the installation, and a client API for deleting the current installation.

## Installation

### NuGet

[![NuGet](https://img.shields.io/nuget/v/Plugin.Firebase.Installations.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Installations/)

> Install-Package Plugin.Firebase.Installations

## Setup
- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/README.md#basic-setup).
- No service-specific initialization is required beyond `CrossFirebase.Initialize(...)`.

```c#
using Plugin.Firebase.Installations;

var installationId = await CrossFirebaseInstallations.Current.GetIdAsync();
var token = await CrossFirebaseInstallations.Current.GetTokenAsync();
var refreshedToken = await CrossFirebaseInstallations.Current.GetTokenAsync(forceRefresh: true);
```

## Delete the current installation

`DeleteAsync()` deletes the current Firebase installation data from the client and Firebase backend. Firebase may create a new installation ID later if another Firebase service needs one.

```c#
await CrossFirebaseInstallations.Current.DeleteAsync();
```

Deleting an installation can affect Firebase services that identify app instances by Firebase installation ID, including Cloud Messaging, Remote Config, Analytics, A/B Testing, and In-App Messaging. Use it only for explicit user or test flows where resetting the installation is intended.

## Further reading

- [Manage Firebase installations](https://firebase.google.com/docs/projects/manage-installations)
- [Android FirebaseInstallations reference](https://firebase.google.com/docs/reference/android/com/google/firebase/installations/FirebaseInstallations)
- [Apple Installations reference](https://firebase.google.com/docs/reference/swift/firebaseinstallations/api/reference/Classes/Installations)

## Release notes

- Next
  - Initial public release targeting .NET 10 with Firebase iOS 12.7, iOS 15, and Android 23 as its initial baselines.
  - Add installation ID retrieval, auth-token retrieval and forced refresh, and installation deletion.

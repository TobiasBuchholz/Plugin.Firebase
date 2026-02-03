# App Check

Firebase App Check helps protect backend resources from abuse by ensuring requests come from your authentic app.

## Installation
### Nuget
> Install-Package Plugin.Firebase.AppCheck

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup).
- Configure App Check before Firebase initialization:

```c#
using Plugin.Firebase.AppCheck;

CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);
```

### Providers
- `Disabled`: No App Check provider is installed.
- `Debug`: Debug provider (iOS / Android).
- `DeviceCheck`: Apple DeviceCheck provider (iOS only).
- `AppAttest`: Apple App Attest provider (iOS only, iOS 14+).
- `PlayIntegrity`: Google Play Integrity provider (Android only).

### iOS enablement
App Check for iOS is enabled by default in `Plugin.Firebase.AppCheck` and uses `AdamE.Firebase.iOS.AppCheck` (`12.5.0.4-fork`).
Make sure your NuGet sources include the feed where that forked package is published.

Configuring a provider that is not supported on the current platform throws a `NotSupportedException`.

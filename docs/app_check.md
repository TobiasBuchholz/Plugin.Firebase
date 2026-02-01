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

### Modes
- `Disabled`: No App Check provider is installed.
- `Debug`: Uses the platform debug provider.
- `Production`: Uses App Attest (iOS, with DeviceCheck fallback) and Play Integrity (Android).

### iOS enablement
App Check for iOS is gated behind `EnableFirebaseAppCheckIos=true` until the `AdamE.Firebase.iOS.AppCheck` package is published for the current Firebase SDK line.

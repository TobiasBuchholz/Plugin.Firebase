# Crashlytics

Firebase [Firebase Crashlytics](https://firebase.google.com/docs/crashlytics) is a lightweight, realtime crash reporter that helps you track, prioritize, and fix stability issues that erode your app quality. Crashlytics saves you troubleshooting time by intelligently grouping crashes and highlighting the circumstances that lead up to them.

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Initialize CrossFirebase with Crashlytics enabled:
```c#
  CrossFirebase.Initialize(..., new CrossFirebaseSettings(isCrashlyticsEnabled:true));
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

Take a look at the [documentation](https://github.com/xamarin/GoogleApisForiOSComponents/blob/master/docs/Firebase/Crashlytics/GettingStarted.md) for the Xamarin.Firebase.iOS.Crashlytics packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following class:
- [src/.../IFirebaseCrashlytics.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Crashlytics/IFirebaseCrashlytics.cs)

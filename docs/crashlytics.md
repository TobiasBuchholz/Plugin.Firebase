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
- Make sure to start your app without the debugger attached when testing fatal crashes.
- After forcing a crash, restart the app so Crashlytics can send the report. The crash may take several minutes to appear in the Firebase Console.
- To enable verbose Firebase logs while diagnosing setup issues, add `-FIRDebugEnabled` to your iOS app launch arguments.
- If the device log contains `Crashlytics could not find the symbol for the app's main function`, add the following properties to your app project:
```xml
<PropertyGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">
    <_ExportSymbolsExplicitly>false</_ExportSymbolsExplicitly>
    <DebugType>full</DebugType>
</PropertyGroup>
```
- If the symbol error still appears, create `Platforms/iOS/exported_symbols.txt` containing exactly:
```text
__mh_execute_header
```
Then reference that file from your app project:
```xml
<ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">
    <_ReferencesLinkerFlags Include="-u__mh_execute_header" Visible="false" />
    <_CustomLinkFlags Include="-exported_symbols_list" Visible="false" />
    <_CustomLinkFlags Include="$(ProjectDir)Platforms/iOS/exported_symbols.txt" Visible="false" />
</ItemGroup>
```
The exported symbols file must be plain UTF-8 without a byte order mark or other hidden characters. A hidden character before `__mh_execute_header` can cause Release builds to fail during linking.
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
- If you created `strings.xml` manually, make sure it is included as an Android resource. In Visual Studio this is the `AndroidResource` build action. If the file is not already included in your project file, add an item like:
```xml
<ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
    <AndroidResource Include="Platforms/Android/Resources/values/strings.xml" />
</ItemGroup>
```
- After adding or changing Android resources, clean `bin` and `obj` and rebuild the app if Crashlytics still reports a missing build ID.
- Plugin.Firebase's default Android Firebase baseline is Firebase BoM 33.0 / `Xamarin.Firebase.Crashlytics` 119.0.0. If you explicitly manage Android Firebase package versions, choose a coherent Firebase BoM and align all `Xamarin.Firebase.*` package versions to that BoM.
- `Xamarin.Firebase.Crashlytics` 119.1.0 and later changed the .NET binding shape for `SetCrashlyticsCollectionEnabled` to prefer `Java.Lang.Boolean`. Plugin.Firebase versions containing the Android binding compatibility fix compile against tested Crashlytics 19.x packages up to 119.4.4.
- Crashlytics 20.x maps to the Firebase BoM 34.x dependency line. Plugin.Firebase versions containing the Android binding compatibility fix are compile-compatible in targeted validation against tested Crashlytics 20.x packages up to 120.0.5, but BoM 34.x should be treated as an intentional full Firebase Android dependency upgrade rather than Plugin.Firebase's default baseline.
- For more specific instructions take a look at the official [Firebase documentation](https://firebase.google.com/docs/crashlytics/get-started?platform=android)

## Usage

To test if everything is setup correctly, restart the app after a forced crash and visit the [Crashlytics Dashboard](https://console.firebase.google.com/u/0/project/_/crashlytics) to view your reports and statistics. See the official Firebase [test implementation guide](https://firebase.google.com/docs/crashlytics/test-implementation?platform=ios) for current platform-specific testing guidance.

Take a look at the [documentation](https://github.com/AdamEssenmacher/GoogleApisForiOSComponents/blob/master/docs/Firebase/Crashlytics/GettingStarted.md) for the AdamE.Firebase.iOS.Crashlytics packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following class:
- [src/.../IFirebaseCrashlytics.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Crashlytics/IFirebaseCrashlytics.cs)

## Release notes
- Version 3.1.1
  - Using AdamE.Firebase.iOS.* minimum version 11
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

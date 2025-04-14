# Plugin.Firebase

This is a wrapper library around the native Android and iOS Firebase Xamarin SDKs which includes cross-platform APIs for most of the Firebase features. Documentation and the included sample app are MAUI-centric, but the plugin should be usable in any cross-platform .NET6+ project.

## Supported features

| Feature | Plugin | Version |
|---------|--------|---------|
| [Analytics](https://firebase.google.com/docs/analytics) | [Plugin.Firebase.Analytics](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/docs/analytics.md) | [![NuGet](https://img.shields.io/nuget/v/plugin.firebase.analytics.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Analytics/) |
| [Auth](https://firebase.google.com/docs/auth) | [Plugin.Firebase.Auth](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/docs/auth.md) | [![NuGet](https://img.shields.io/nuget/v/plugin.firebase.auth.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Auth/) |
| [Facebook Sign In](https://developers.facebook.com/docs/facebook-login/) | [Plugin.Firebase.Auth.Facebook](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/docs/auth_facebook.md) | [![NuGet](https://img.shields.io/nuget/v/plugin.firebase.auth.facebook.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Auth.Facebook/) |
| [Google Sign In](https://developers.google.com/identity/authentication) | [Plugin.Firebase.Auth.Google](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/docs/auth.md) | [![NuGet](https://img.shields.io/nuget/v/plugin.firebase.auth.google.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Auth.Google/) |
| [Cloud Messaging](https://firebase.google.com/docs/cloud-messaging) | [Plugin.Firebase.CloudMessaging](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/docs/cloud_messaging.md) | [![NuGet](https://img.shields.io/nuget/v/plugin.firebase.cloudmessaging.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.CloudMessaging/)
| [Crashlytics](https://firebase.google.com/docs/crashlytics) | [Plugin.Firebase.Crashlytics](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/docs/crashlytics.md) | [![NuGet](https://img.shields.io/nuget/v/plugin.firebase.crashlytics.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Crashlytics/)
| [Dynamic Links](https://firebase.google.com/docs/dynamic-links) | [Plugin.Firebase.DynamicLinks](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/docs/dynamic_links.md) | [![NuGet](https://img.shields.io/nuget/v/plugin.firebase.dynamiclinks.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.DynamicLinks/)
| [Firestore](https://firebase.google.com/docs/firestore) | [Plugin.Firebase.Firestore](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/docs/firestore.md) | [![NuGet](https://img.shields.io/nuget/v/plugin.firebase.firestore.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Firestore/)
| [Cloud Functions](https://firebase.google.com/docs/functions) | [Plugin.Firebase.Functions](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/docs/functions.md) | [![NuGet](https://img.shields.io/nuget/v/plugin.firebase.functions.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Functions/)
| [Remote Config](https://firebase.google.com/docs/remote-config) | [Plugin.Firebase.RemoteConfig](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/docs/remote_config.md) | [![NuGet](https://img.shields.io/nuget/v/plugin.firebase.remoteconfig.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.RemoteConfig/)
| [Storage](https://firebase.google.com/docs/storage) | [Plugin.Firebase.Storage](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/docs/storage.md) | [![NuGet](https://img.shields.io/nuget/v/plugin.firebase.storage.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Storage/)
| All in one | [Plugin.Firebase](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/docs/bundled.md) | [![NuGet](https://img.shields.io/nuget/v/plugin.firebase.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase/)

## Basic setup
This plugin is a thin wrapper around the native binding packages, which are, in turn, wrappers around the native platform SDKs. As such, developers should not primarily rely on documentation in this plugin for Firebase project configuration or SDK usage. [The official Firebase docs](https://firebase.google.com/docs) should be your primary Firebase resource.

1. Create a Firebase project in the [Firebase Console](https://console.firebase.google.com/), if you don't already have one. If you already have an existing Google project associated with your mobile app, click **Import Google Project**. Otherwise, click **Create New Project**.
2. Click **Add Firebase to your *[iOS|Android]* app** and follow the setup steps. If you're importing an existing Google project, this may happen automatically and you can just download the config file.
3. Add `[GoogleService-Info.plist|google-services.json]` file to your app project.
4. Set `[GoogleService-Info.plist|google-services.json]` **build action** behaviour to `[Bundle Resource|GoogleServicesJson]` by Right clicking/Build Action.
5. **IMPORTANT (iOS only)** Check the Readme at [AdamEssenmacher/GoogleApisForiOSComponents](https://github.com/AdamEssenmacher/GoogleApisForiOSComponents) for additional setup steps that are required.
6. Additional setup instructions for individual components (e.g. Auth, Analytics, Firestore, etc.) are linked in the above table.

### Dependency Management
The native Firebase SDKs for both Android and iOS have their own set of dependencies--some of which are shared by other common Google libraries. To maximize compatibility and give developers the flexibility to choose from a range of compatible native SDK versions, this plugin's NuGet configuration specifies [version ranges](https://learn.microsoft.com/en-us/nuget/concepts/package-versioning?tabs=semver20sort#version-ranges) for the native SDK binding libraries. When resolving dependencies, NuGet's standard behavior is to resolve _minimum_ compatible versions. This ultimately means that developers are responsible for managing versions of the Firebase iOS/Android SDKs that are included in their projects. This is done by adding the appropriate `AdamE.Firebase.iOS.*` and `Xamarin.Firebase.*` NuGet package references to your project. This is not strictly required for the plugin to work, but it is recommended to ensure that you are using the latest versions of the Firebase SDKs, which may include bug fixes or performance improvements internal to the Firebase SDKs.

When new major versions of native SDKs are released, Plugin.Firebase _may or may not_ be compatible with them. It depends on whether or not breaking API changes have been introduced. In such cases, please open an issue (or PR) and the plugin will be updated to support the new version.

The native binding packages are published using a versioning scheme `[MAJOR].[MINOR].[PATCH].[REVISION]`. The MAJOR, MINOR, and PATCH version numbers mirror the native Firebase SDK versions. The REVISION number is incremented if the bindings themselves are updated. (Note: On Android, a '1' is prefixed to the MAJOR version. For example, native Android SDK 24.1.0 is published as 124.1.0.)

#### iOS Dependencies

When selecting package versions for iOS, best practice is to make sure that the MAJOR and MINOR versions match across all packages (e.g. 11.8.*.*). Then, for each package, select the latest PATCH.REVISION.

- AdamE.Firebase.iOS.Analytics
- AdamE.Firebase.iOS.Auth
- AdamE.Firebase.iOS.CloudMessaging
- AdamE.Firebase.iOS.Core
- AdamE.Firebase.iOS.Crashlytics
- AdamE.Firebase.iOS.DynamicLinks
- AdamE.Firebase.iOS.Firestore
- AdamE.Firebase.iOS.Functions
- AdamE.Firebase.iOS.RemoteConfig
- AdamE.Firebase.iOS.Storage

#### Android Dependencies

When selecting package versions for Android, best practice is to [choose a Firebase BoM](https://firebase.google.com/support/release-notes/android) and select matching package versions for each dependency. As examples, if you wish to use BoM version 33.8.0, then you would want to select Xamarin.Firebase.Analytics v122.2.0 and Xamarin.Firebase.Auth v123.1.0.

- Xamarin.Firebase.Analytics
- Xamarin.Firebase.Auth
- Xamarin.Firebase.Messaging
- Xamarin.Firebase.Common
- Xamarin.Firebase.Crashlytics
- Xamarin.Firebase.DynamicLinks
- Xamarin.Firebase.Firestore
- Xamarin.Firebase.Functions
- Xamarin.Firebase.Config
- Xamarin.Firebase.Storage

### MAUI Project Configuration
To get started add the `GoogleService-Info.plist` and the `google-services.json` files to the root folder of your project and include them in the .csproj file like this:

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0-android'">
    <GoogleServicesJson Include="google-services.json" />
</ItemGroup>

<ItemGroup Condition="'$(TargetFramework)' == 'net8.0-ios'">
    <BundleResource Include="GoogleService-Info.plist" />
</ItemGroup>
```

Be sure to change the `TargetFramework` condition to the correct value for your target versions (i.e. `net6.0-*`, `net7.0-*`, `net8.0-*`, etc.)

Initialize the plugin in your `MauiProgram.cs` like this:

```c#
using Plugin.Firebase.Auth;

#if IOS
using Plugin.Firebase.Core.Platforms.iOS;
#elif ANDROID
using Plugin.Firebase.Core.Platforms.Android;
#endif

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        return MauiApp
            .CreateBuilder()
            .UseMauiApp<App>()
            ...
            .RegisterFirebaseServices()
            .Build();
    }
    
    private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events => {
#if IOS
            events.AddiOS(iOS => iOS.WillFinishLaunching((_,__) => {
                CrossFirebase.Initialize();
                return false;
            }));
#elif ANDROID
            events.AddAndroid(android => android.OnCreate((activity, _) =>
                CrossFirebase.Initialize(activity)));
#endif
        });
        
        builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);
        return builder;
    }
    
}
```
Ensure the `ApplicationId` in your `.csproj` file matches the `bundle_id` and `package_name` inside of the `[GoogleService-Info.plist|google-services.json]` files:
```xml
<ApplicationId>com.example.myapp</ApplicationId>
```

The plugin doesn't support Windows or Mac catalyst, so either remove their targets from your `.csproj` file or use  [preprocessor directives](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives#conditional-compilation) and [MSBuild conditions](https://learn.microsoft.com/de-de/visualstudio/msbuild/msbuild-conditions?view=vs-2022), e.g:

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0-ios' OR '$(TargetFramework)' == 'net8.0-android'">
    <PackageReference Include="Plugin.Firebase" Version="1.2.0" />
</ItemGroup>
```

### Android specifics
- For package versions prior to `Plugin.Firebase 2.0.7`, `Plugin.Firebase.Auth 2.0.5`, `Plugin.Firebase.Firestore 2.0.5`, `Plugin.Firebase.Functions 2.0.2` or `Plugin.Firebase.Storage 2.0.2` add the following `ItemGroup` to your `.csproj` file to prevent build errors:
```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0-android'">
  <PackageReference Include="Xamarin.Kotlin.StdLib.Jdk7" Version="1.7.10" ExcludeAssets="build;buildTransitive" />
  <PackageReference Include="Xamarin.Kotlin.StdLib.Jdk8" Version="1.7.10" ExcludeAssets="build;buildTransitive" />
</ItemGroup>
```
- For versions < 3.1.0 add the following `ItemGroup` to your `.csproj` file to prevent build errors:
```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0-android'">
    <PackageReference Include="Xamarin.AndroidX.Core" Version="1.12.0.2" />
    <PackageReference Include="Xamarin.AndroidX.Collection" Version="1.3.0.1" />
    <PackageReference Include="Xamarin.AndroidX.Collection.Ktx" Version="1.3.0.1" />
    <PackageReference Include="Xamarin.AndroidX.Activity.Ktx" Version="1.8.0.1" />
    <PackageReference Include="Xamarin.AndroidX.Browser" Version="1.6.0.2" />
</ItemGroup>
```

- For versions >= 3.1.0 that use `Plugin.Firebase.Firestore` add:
```xml
<ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
    <PackageReference Include="Xamarin.AndroidX.Core" Version="1.15.0.1" />
    <PackageReference Include="Xamarin.AndroidX.Collection" Version="1.4.5.1" />
    <PackageReference Include="Xamarin.AndroidX.Collection.Ktx" Version="1.4.5.1" />
    <PackageReference Include="Xamarin.AndroidX.Activity.Ktx" Version="1.9.3.1" />
    <PackageReference Include="Xamarin.AndroidX.Browser" Version="1.8.0.7" />
    <PackageReference Include="Xamarin.AndroidX.Lifecycle.LiveData.Core" Version="2.8.7.1" />
    <PackageReference Include="Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx" Version="2.8.7.1" />
</ItemGroup>
```

Take a look at the [sample project](https://github.com/TobiasBuchholz/Plugin.Firebase/tree/development/sample/Playground) to get more information.

## Plugin.Firebase.Legacy
If you are working with an older Xamarin project and are not able to migrate to .NET MAUI yet, there is a legacy version of the plugin called [Plugin.Firebase.Legacy](https://www.nuget.org/packages/Plugin.Firebase.Legacy/). The code for this package is located on a branch called `legacy`. Bugfixes or other small important changes can be done here and will be synced to the `development/master` branch if needed.

### Android specifics
- Add the following `PackageReference` to the `.csproj file` of your android project to prevent a build error (see this [github comment](https://github.com/xamarin/GooglePlayServicesComponents/issues/379#issuecomment-733266753) for more information):
```xml
<PackageReference Include="Xamarin.Google.Guava.ListenableFuture" Version="1.0.0.2" ExcludeAssets="build;buildTransitive" />
```
- If you receive an error that states the `default Firebase App is not initialized`, adding one package explicitly seems to resolve this issue (it doesn't seem to matter which package gets added).

## Documentation and samples

In the [docs folder](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/docs) you can find for every feature a designated readme file that describes the setup and usage of this feature or where to find more information.

In the [sample folder](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/sample) you can find a sample Xamarin.Forms project. This project serves as a base to play around with the plugin and to test features that are hard to test automatically (like Authentication or Cloud Messages). [playground-functions](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/sample/playground-functions) is a Cloud Functions project and contains the code to enable sending Cloud Messages from the backend.

In the [tests folder](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/tests) you can find a Xamarin.Forms project that lets you run integration tests. You should definitely check out the ```*Fixture.cs``` files to learn how the plugin is supposed to work. All the tests should pass when they get executed on a real device.

In case you would like to run the sample or test project by yourself, you need to add the `GoogleService-Info.plist` and `google-services.json` files of your own firebase project and adapt the other config files like `Info.plist, Entitlements.plist, AndroidManifest.xml`.

#### For windows users
@coop-tim has created a [.NET 8 sample project](https://github.com/coop-tim/maui-sample) with a really good and extensive description. It's targeting iOS and Android with Firebase Cloud Messaging, Analytics and the relevant build pipelines to get them built and pushed into App Center. Definitely worth checking it out!

## Using Firebase Local Emulator Suite
If you would like to use the [Firebase Local Emulator Suite](https://firebase.google.com/docs/emulator-suite) for your tests or rapid prototyping you can do so by following the steps of the [Getting started guide](https://firebase.google.com/docs/emulator-suite/connect_and_prototype) and calling the [`UseEmulator(host, port)`](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/src/Shared/Firestore/IFirebaseFirestore.cs#L45) method of the desired firebase service before doing any other operations.

For example the [`Plugin.Firebase.IntegrationTests`](https://github.com/TobiasBuchholz/Plugin.Firebase/tree/development/tests/Plugin.Firebase.IntegrationTests) project is configured to be able to use the [Cloud Firestore emulator](https://firebase.google.com/docs/emulator-suite/connect_firestore). You can start the emulator with initial seed data by running `firebase emulators:start --only firestore --import=test-data/`. Uncomment the line `CrossFirebaseFirestore.Current.UseEmulator("localhost", 8080);` in [`IntegrationTestAppDelegate.cs`](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/tests/Plugin.Firebase.TestHarness.iOS/IntegrationTestAppDelegate.cs) or [`MainActivity.cs`](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/tests/Plugin.Firebase.TestHarness.Android/MainActivity.cs) to let the test project know it should use the emulator. Now all firestore related tests should pass.

## Troubleshooting
### Windows + Visual Studio + iOS long path issue
Visual Studio and Windows both have issues with long paths. The issue is explained (along with best-known workarounds) [here](https://github.com/AdamEssenmacher/GoogleApisForiOSComponents?tab=readme-ov-file#windows--visual-studio-long-path-issue).

## Contributions

You are welcome to contribute to this project by creating a [Pull Request](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests). The project contains an .editorconfig file that handles the code formatting, so please apply the formatting rules by running `dotnet format src/Plugin.Firebase.sln` in the console before creating a Pull Request (see [dotnet-format docs](https://github.com/dotnet/format) or [this video](https://www.youtube.com/watch?v=lGvP9SZ98vM&t) for more information).

## License

`Plugin.Firebase` is released under the MIT license. See the [LICENSE](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/LICENSE) file for details.

## Release notes
- Version 2.0.*
  - see docs of separate nuget packages
- Version 2.0.0
  - All features have been split into separate nuget packages 
  - Cleaned up namespaces
  - Enable usage of `DateTime` in Firestore #137
  - Remove unnecessary dependency `Microsoft.CSharp` #143
  - Fix fcm token refresh by adding `DidReceiveRegistrationToken` method to `FirebaseCloudMessagingImplementation`
- Version 1.3.0
  - Get rid of newtonsoft.json, use system.text.json instead #119
  - Add crashlytics implementation #120
  - Add support for DateTime type to ToHashMap/Put extension #121
  - Lazy google/facebook auth #122
  - Return FIRAuthError.WrongPassword at android for wrong pw signin #117
  - Calling completion in didReceiveNotificationResponse:completionHandler #106
  - Big thanks to tranb3r for the contributions! :)

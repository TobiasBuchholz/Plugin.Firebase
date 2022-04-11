# Plugin.Firebase

This is a wrapper library around the native Android and iOS Firebase Xamarin SDKs. It includes cross-platform APIs for Firebase [Analytics](https://firebase.google.com/docs/analytics), [Auth](https://firebase.google.com/docs/auth), [Cloud Messaging](https://firebase.google.com/docs/cloud-messaging), [Dynamic Links](https://firebase.google.com/docs/dynamic-links), [Firestore](https://firebase.google.com/docs/firestore), [Cloud Functions](https://firebase.google.com/docs/functions), [Remote Config](https://firebase.google.com/docs/remote-config) and [Storage](https://firebase.google.com/docs/storage).

## Installation
#### Nuget
[![NuGet](https://img.shields.io/nuget/v/plugin.firebase.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase/)

> Install-Package Plugin.Firebase

## Basic setup

1. Create a Firebase project in the [Firebase Console](https://console.firebase.google.com/), if you don't already have one. If you already have an existing Google project associated with your mobile app, click **Import Google Project**. Otherwise, click **Create New Project**.
2. Click **Add Firebase to your *[iOS|Android]* app** and follow the setup steps. If you're importing an existing Google project, this may happen automatically and you can just download the config file.
3. Add ```[GoogleService-Info.plist|google-services.json]``` file to your app project.
4. Set ```[GoogleService-Info.plist|google-services.json]``` **build action** behaviour to ```[Bundle Resource|GoogleServicesJson]``` by Right clicking/Build Action.
5. Add the following line of code to the place where your app gets bootstrapped:
```c#
  CrossFirebase.Initialize(..., new CrossFirebaseSettings(...));
```

### Android specifics
- Add the following `PackageReference` to the `.csproj file` of your android project to prevent a build error (see this [github comment](https://github.com/xamarin/GooglePlayServicesComponents/issues/379#issuecomment-733266753) for more information):
```xml
<PackageReference Include="Xamarin.Google.Guava.ListenableFuture" Version="1.0.0.2" ExcludeAssets="build;buildTransitive" />
```
- If you receive an error that states the `default Firebase App is not initialized`, adding one package explicitly seems to resolve this issue (it doesn't seem to matter which package gets added).

## Documentation and samples

In the [docs folder](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/docs) you can find for every feature a designated readme file that describes the setup and usage of this feature or where to find more information.

In the [sample folder](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/sample) you can find a sample Xamarin.Forms project. This project serves as a base to play around with the plugin and to test features that are hard to test automatically (like Authentication or Cloud Messages). [playground-functions](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/sample/playground-functions) is a Cloud Functions project and contains the code to enable sending Cloud Messages from the backend.

In the [tests folder](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests) you can find a Xamarin.Forms project that lets you run integration tests. You should definitely check out the ```*Fixture.cs``` files to learn how the plugin is supposed to work. All the tests should pass when they get executed on a real device.

In case you would like to run the sample or test project by yourself, you need to add the ```GoogleService-Info.plist``` and ```google-services.json``` files of your own firebase project and adapt the other config files like ```Info.plist, Entitlements.plist, AndroidManifest.xml```.

## Using Firebase Local Emulator Suite
If you would like to use the [Firebase Local Emulator Suite](https://firebase.google.com/docs/emulator-suite) for your tests or rapid prototyping you can do so by following the steps of the [Getting started guide](https://firebase.google.com/docs/emulator-suite/connect_and_prototype) and calling the [`UseEmulator(host, port)`](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/src/Shared/Firestore/IFirebaseFirestore.cs#L45) method of the desired firebase service before doing any other operations.

For example the [`Plugin.Firebase.IntegrationTests`](https://github.com/TobiasBuchholz/Plugin.Firebase/tree/development/tests/Plugin.Firebase.IntegrationTests) project is configured to be able to use the [Cloud Firestore emulator](https://firebase.google.com/docs/emulator-suite/connect_firestore). You can start the emulator with initial seed data by running `firebase emulators:start --only firestore --import=test-data/`. Uncomment the line `CrossFirebaseFirestore.Current.UseEmulator("localhost", 8080);` in [`IntegrationTestAppDelegate.cs`](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/tests/Plugin.Firebase.TestHarness.iOS/IntegrationTestAppDelegate.cs) or [`MainActivity.cs`](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/tests/Plugin.Firebase.TestHarness.Android/MainActivity.cs) to let the test project know it should use the emulator. Now all firestore related tests should pass.

## Contributions

You are welcome to contribute to this project by creating a [Pull Request](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests). The project contains an .editorconfig file that handles the code formatting, so please apply the formatting rules by running `dotnet format src/Plugin.Firebase.sln` in the console before creating a Pull Request (see [dotnet-format docs](https://github.com/dotnet/format) or [this video](https://www.youtube.com/watch?v=lGvP9SZ98vM&t) for more information).

## License

```Plugin.Firebase``` is released under the MIT license. See the [LICENSE](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/LICENSE) file for details.

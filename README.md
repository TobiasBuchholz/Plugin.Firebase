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

## Documentation and samples

In the [docs folder](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/docs) you can find for every feature a designated readme file that describes the setup and usage of this feature or where to find more information.

In the [sample folder](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/sample) you can find a sample Xamarin.Forms project. This project serves as a base to play around with the plugin and to test features that are hard to test automatically (like Authentication or Cloud Messages). [playground-functions](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/sample/playground-functions) is a Cloud Functions project and contains the code to enable sending Cloud Messages from the backend.

In the [tests folder](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests) you can find a Xamarin.Forms project that lets you run integration tests. You should definitely check out the ```*Fixture.cs``` files to learn how the plugin is supposed to work. All the tests should pass when they get executed on a real device.

In case you would like to run the sample or test project by yourself, you need to add the ```GoogleService-Info.plist``` and ```google-services.json``` files of your own firebase project and adapt the other config files like ```Info.plist, Entitlements.plist, AndroidManifest.xml```.

## License

```Plugin.Firebase``` is released under the MIT license. See the [LICENSE](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/LICENSE) file for details.

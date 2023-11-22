# Auth

You can use [Firebase Authentication](https://firebase.google.com/docs/auth) to allow users to sign in to your app using one or more sign-in methods, including email address and password sign-in, and federated identity providers such as Google Sign-in and Facebook Login.

## Installation
### Nuget
[![NuGet](https://img.shields.io/nuget/v/plugin.firebase.auth.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Auth/)

> Install-Package Plugin.Firebase.Auth

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Enable Authentication at your project in the [Firebase Console](https://console.firebase.google.com/).
- When using [Plugin.Firebase.Auth.Google](https://www.nuget.org/packages/Plugin.Firebase.Auth.Google/), add the following lines of code after calling `CrossFirebase.Initialize()`:
```c#
#if IOS
  FirebaseAuthGoogleImplementation.Initialize();
#elif ANDROID
  FirebaseAuthGoogleImplementation.Initialize("your-google-request-id-token");
#endif
```
- The `googleRequestIdToken` can be accessed at the [Google API Console](https://console.developers.google.com/apis/credentials) (make sure to use the Client-ID of the Web client)

### iOS specifics
- Enable keychain entitlement in Entitlements.plist:

```xml
  <dict>
    <key>keychain-access-groups</key>
    <array>
      <string>$(AppIdentifierPrefix)my.fancy.app</string>
    </array>
  </dict>
```
- In case you are using Authentication via Google, add an url scheme to your apps `Info.plist`:
```xml
  <key>CFBundleURLTypes</key>
  <array>
    <dict>
      <key>CFBundleURLSchemes</key>
      <array>
        <string>com.googleusercontent.apps.123456-abcdef</string>
      </array>
    </dict>
  </array>
```
- For more specific instructions take a look at the official [Firebase documentation](https://firebase.google.com/docs/auth/ios/start?hl=en)

### Android specifics

- Make sure you have added the SHA-1 fingerprint matching the keystore you've used for signing the app bundle to the android project settings in the firebase console: 

![firestore_poco.png](../art/project_settings_sha1.png)
- Call `FirebaseAuthImplementation.HandleActivityResultAsync(requestCode, resultCode, data);` from `MainActivity.OnActivityResult(...)`
- If you are on version 2.0.5 or later, add the following package to your project's `.csproj` file to prevent build errors:
```xml
  <PackageReference Include="Xamarin.AndroidX.Browser" Version="1.6.0.2" />
```

For more specific instructions take a look at the official [Firebase documentation](https://firebase.google.com/docs/auth/android/start?hl=en)

## Usage

Take a look at the [documentation](https://github.com/xamarin/GoogleApisForiOSComponents/blob/master/docs/Firebase/Auth/GettingStarted.md) for the Xamarin.Firebase.iOS.Auth packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseAuth.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Auth/IFirebaseAuth.cs)
- [src/.../IFirebaseUser.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Auth/IFirebaseUser.cs)
- [tests/.../AuthFixture.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/Plugin.Firebase.IntegrationTests/Auth/AuthFixture.cs)
- [sample/.../AuthService.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/sample/Playground/Common/Services/Auth/AuthService.cs)

## Release notes
- Version 2.0.5
  - Bump up Xamarin.Firebase.Auth from 121.0.8 to 122.2.0 (issue #131)
- Version 2.0.4
  - Separating Auth.Google into its own package (PR #210)
- Version 2.0.3
  - Return `FirebaseUser` in `FirebaseAuthImplementation.CreateUserAsync(email, password)` method (issue #183)
- Version 2.0.2
  - Bump up Xamarin.Google.iOS.SignIn from 5.0.2.3 to 5.0.2.4 (issue #158)
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net6.0 tfm

# Dynamic Links

With Firebase Dynamic Links, you can give new users of your app a personalized onboarding experience, and thus increase user sign-ups, user retention, and long-term user engagement.

Dynamic Links are links into an app that work whether or not users have installed the app yet. When users open a Dynamic Link into an app that is not installed, the app's App- or Playstore page opens, where users can install the app. After users install and open the app, the app handles the link.

## Installation
### Nuget
[![NuGet](https://img.shields.io/nuget/v/plugin.firebase.dynamic_links.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.DynamicLinks/)

> Install-Package Plugin.Firebase.DynamicLinks

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Enable Dynamic Links at your project in the [Firebase Console](https://console.firebase.google.com/)

### iOS specifics
- Go to the firebase console -> Project Settings -> choose your iOS app -> enter Appstore ID and Team ID (if not filled out already)
- Go to developers.apple.com -> Certificates, Identifiers & Profiles -> App IDs -> choose your app and enable 'Associated Domains'
- Go to provisioning profiles -> choose your profile, make it valid again, download and double tap it
- add associated domains to your apps `Entitlements.plist`:
```xml
  <key>com.apple.developer.associated-domains</key>
  <array>
    <string>applinks:myfancyapp.page.link</string>
  </array>
```
- add url scheme to your apps ```Info.plist```:
```xml
  <key>CFBundleURLTypes</key>
  <array>
    <dict>
      <key>CFBundleURLName</key>
      <string>Bundle ID</string>
      <key>CFBundleURLSchemes</key>
      <array>
        <string>my.fancy.app</string>
      </array>
      <key>CFBundleURLTypes</key>
      <string>Editor</string>
    </dict>
  </array>
```
- For more specific instructions take a look at the official [Firebase documentation](https://firebase.google.com/docs/dynamic-links?hl=en)

### Android specifics
- use the following command to generate SHA-1 and SHA-256 fingerprints:
```
  keytool -exportcert -list -v -alias <your-key-name> -keystore <path-to-production-keystore>
```
- Go to firebase console -> Project Settings -> choose your android app -> insert the SHA-1 and SHA-256 fingerprints
- Call `FirebaseDynamicLinksImplementation.HandleDynamicLinkAsync(intent)` from `MainActivity.OnCreate(...)` and `MainActivity.OnNewIntent(...)`
- Add your `MainActivity` to the `<application>` tag in your apps `AndroidManifest.xml`:
```xml
  <activity
    android:name="my.fancy.app.MainActivity"
    android:label="@string/app_name"
    android:icon="@mipmap/icon"
    android:theme="@style/MainTheme"
    android:configChanges="screenSize|orientation|uiMode|screenLayout|smallestScreenSize"
    android:launchMode="singleTask">
    <intent-filter>
      <action android:name="android.intent.action.MAIN" />
      <category android:name="android.intent.category.LAUNCHER" />
    </intent-filter>
    <intent-filter>
      <action android:name="android.intent.action.VIEW" />
      <category android:name="android.intent.category.DEFAULT" />
      <category android:name="android.intent.category.BROWSABLE" />
      <data android:host="myfancyapp.page.link" android:scheme="https" />
    </intent-filter>
  </activity>
```
- For more specific instructions take a look at the official [Firebase documentation](https://firebase.google.com/docs/dynamic-links?hl=en)

## Usage

Take a look at the [documentation](https://github.com/xamarin/GoogleApisForiOSComponents/blob/master/docs/Firebase/DynamicLinks/GettingStarted.md) for the Xamarin.Firebase.iOS.DynamicLinks packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseDynamicLinks.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/DynamicLinks/IFirebaseDynamicLinks.cs)
- [tests/.../DynamicLinksFixture.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/Plugin.Firebase.IntegrationTests/DynamicLinks/DynamicLinksFixture.cs)
- [sample/.../DynamicLinkService.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/sample/Playground/Common/Services/DynamicLink/DynamicLinkService.cs)

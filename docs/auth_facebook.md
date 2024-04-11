# Auth Facebook
You can use [Facebook Authentication](https://developers.facebook.com/docs/facebook-login/) to allow users to sign in to your app using their facebook account.

## Installation
### Nuget
[![NuGet](https://img.shields.io/nuget/v/plugin.firebase.auth_facebook.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Auth.Facebook/)

> Install-Package Plugin.Firebase.Auth.Facebook

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Enable Authentication at your project in the [Firebase Console](https://console.firebase.google.com/).
- Add the following lines of code after calling `CrossFirebase.Initialize()`:
```c#
#if IOS
  FirebaseAuthFacebookImplementation.Initialize(app, launchOptions, "your-facebook-id", "your-facebook-app-name");
#endif
```
- The `facebookId` and `facebookAppName` can be accessed at [Facebook Developers](https://developers.facebook.com/apps/)

### Android specifics

- Add the following code to your apps `AndroidManifest.xml`:
```xml
  <meta-data android:name="com.facebook.sdk.ApplicationId" android:value="@string/facebook_app_id" />
  <activity android:name="com.facebook.FacebookActivity" android:configChanges="keyboard|keyboardHidden|screenLayout|screenSize|orientation" android:label="@string/app_name" />
  <activity android:name="com.facebook.CustomTabActivity" android:exported="true">
    <intent-filter>
      <action android:name="android.intent.action.VIEW" />
      <category android:name="android.intent.category.DEFAULT" />
      <category android:name="android.intent.category.BROWSABLE" />
      <data android:scheme="@string/fb_login_protocol_scheme" />
    </intent-filter>
  </activity>
```
- Add `facebook_app_id` and `fb_login_protocol_scheme` to `strings.xml`:
```xml
  <string name="facebook_app_id">12345678</string>
  <string name="fb_login_protocol_scheme">fb12345678</string>
```
- Call `FirebaseAuthFacebookImplementation.HandleActivityResultAsync(requestCode, resultCode, data);` from `MainActivity.OnActivityResult(...)`

## Release notes
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net8.0 tfm

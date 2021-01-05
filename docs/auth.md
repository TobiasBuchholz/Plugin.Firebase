# Auth

You can use [Firebase Authentication](https://firebase.google.com/docs/auth) to allow users to sign in to your app using one or more sign-in methods, including email address and password sign-in, and federated identity providers such as Google Sign-in and Facebook Login.

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Enable Authentication at your project in the [Firebase Console](https://console.firebase.google.com/).
- Initialize CrossFirebase with Auth enabled:
```c#
  CrossFirebase.Initialize(..., new CrossFirebaseSettings(isAuthEnabled:true));
```
- If you want to enable authentication via Facebook or Google, initialize CrossFirebase with the appropriate settings:
```c#
  var settings = new CrossFirebaseSettings(
      isAuthEnabled:true,
      facebookId:"12345678",
      facebookAppName:"My fancy app",
      googleRequestIdToken:"123456-abcdef.apps.googleusercontent.com");

```
- The ```facebookId``` and ```facebookAppName``` can be accessed at [Facebook Developers](https://developers.facebook.com/apps/)
- The ```googleRequestIdToken``` can be accessed at the [Google API Console](https://console.developers.google.com/apis/credentials) (make sure to use the Client-ID of the Web client)


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
- In case you are using Authentication via Google, add an url scheme to your apps ```Info.plist```:
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

- Call ```FirebaseAuthImplementation.HandleActivityResultAsync(requestCode, resultCode, data);``` from ```MainActivity.OnActivityResult(...)```
- In case you are using Authentication via Facebook, add the following code to your apps ```AndroidManifest.xml```:
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
- For more specific instructions take a look at the official [Firebase documentation](https://firebase.google.com/docs/auth/android/start?hl=en)

## Usage

Take a look at the [documentation](https://github.com/xamarin/GoogleApisForiOSComponents/blob/master/docs/Firebase/Auth/GettingStarted.md) for the Xamarin.Firebase.iOS.Auth packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseAuth.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Auth/IFirebaseAuth.cs)
- [src/.../IFirebaseUser.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Auth/IFirebaseUser.cs)
- [tests/.../AuthFixture.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/Plugin.Firebase.IntegrationTests/Auth/AuthFixture.cs)
- [sample/.../AuthService.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/sample/Playground/Common/Services/Auth/AuthService.cs)

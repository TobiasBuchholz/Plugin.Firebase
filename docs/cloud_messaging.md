# Cloud Messaging

Firebase Cloud Messaging offers a broad range of messaging options and capabilities. I invite you to read the following [documentation](https://firebase.google.com/docs/cloud-messaging/concept-options) to have a better understanding about notification messages and data messages and what you can do with them using FCM's options.

## Installation
### Nuget
[![NuGet](https://img.shields.io/nuget/v/plugin.firebase.cloud_messaging.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.CloudMessaging/)

> Install-Package Plugin.Firebase.CloudMessaging

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Enable Cloud Messaging at your project in the [Firebase Console](https://console.firebase.google.com/)
- Add the following line of code after calling `CrossFirebase.Initialize()`:
```c#
#if IOS
  FirebaseCloudMessagingImplementation.Initialize();
#endif
```
- Make sure the device is able to receive cloud messages and the user has granted the permissions for it:
```c#
  CrossFirebaseCloudMessaging.Current.CheckIfValid()
```

### iOS specifics
- Go to [developers.apple.com](https://developer.apple.com/) -> Certificates, Identifiers & Profiles -> enable Push Notifications in your provisioning profile, download and double tap it
- [Create and upload APNs authentication key](https://firebase.google.com/docs/cloud-messaging/ios/client#upload_your_apns_authentication_key) to your project in the [Firebase Console](https://console.firebase.google.com/)
- Enable Push Notifications in your apps `Entitlements.plist`:
```xml
  <key>aps-environment</key>
  <string>development</string>
```
- For testing launch the app without the debugger, otherwise the push notification may not be received
- For more specific instructions take a look at the official [Firebase documentation](https://firebase.google.com/docs/cloud-messaging/ios/client?hl=en)


### Android specifics
- Add the following code snippet to the `<application>` tag in your apps `AndroidManifest.xml`:
```xml
  <receiver
    android:name="com.google.firebase.iid.FirebaseInstanceIdInternalReceiver"
    android:exported="false" />
  <receiver
    android:name="com.google.firebase.iid.FirebaseInstanceIdReceiver"
    android:exported="true"
    android:permission="com.google.android.c2dm.permission.SEND">
    <intent-filter>
      <action android:name="com.google.android.c2dm.intent.RECEIVE" />
      <action android:name="com.google.android.c2dm.intent.REGISTRATION" />
      <category android:name="${applicationId}" />
    </intent-filter>
  </receiver>
```
- Call `FirebaseCloudMessagingImplementation.OnNewIntent(intent)` from `MainActivity.OnCreate(...)` and `MainActivity.OnNewIntent(...)`
- Create a notification channel and set the ```ChannelId``` to ```FirebaseCloudMessagingImplementation```:
```c#
  private void CreateNotificationChannel()
  {
      var channelId = $"{Application.Context.PackageName}.general";
      var notificationManager = (NotificationManager) Application.Context.GetSystemService(Context.NotificationService);
      var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
      notificationManager.CreateNotificationChannel(channel);
      FirebaseCloudMessagingImplementation.ChannelId = channelId;
  }
```
- For more specific instructions take a look at the official [Firebase documentation](https://firebase.google.com/docs/cloud-messaging/android/client?hl=en)

## Usage

Take a look at the [documentation](https://github.com/xamarin/GoogleApisForiOSComponents/blob/master/docs/Firebase/CloudMessaging/GettingStarted.md) for the Xamarin.Firebase.iOS.CloudMessaging packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseCloudMessaging.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/CloudMessaging/IFirebaseCloudMessaging.cs)
- [sample/.../PushNotificationService.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/sample/Playground/Common/Services/PushNotification/PushNotificationService.cs)

## Troubleshooting

If you are having trouble receiving push notifications on your device, take a look at this helpful https://github.com/TobiasBuchholz/Plugin.Firebase/issues/145#issuecomment-1455182588 by @andyzukunft.

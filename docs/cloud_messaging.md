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

#### Customize local push notifications
Local push notifications are shown by default in a specific way, but you have the following two options to customize this behavior:

##### Overriding `FirebaseCloudMessagingImplementation.NotificationBuilderProvider`:

```c#
FirebaseCloudMessagingImplementation.NotificationBuilderProvider = notificaion => new NotificationCompat.Builder(context, channelId)
    .SetSmallIcon(Android.Resource.Drawable.SymDefAppIcon)
    .SetContentTitle(notification.Title)
    .SetContentText(notification.Body)
    .SetPriority(NotificationCompat.PriorityDefault)
    .SetAutoCancel(true);
```

##### Setting `FirebaseCloudMessagingImplementation.ShowLocalNotificationAction`:

```c#
FirebaseCloudMessagingImplementation.ShowLocalNotificationAction = notification => {

    var intent = PackageManager.GetLaunchIntentForPackage(PackageName);
    intent.PutExtra(FirebaseCloudMessagingImplementation.IntentKeyFCMNotification, notification.ToBundle());
    intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

    var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
    var builder = new NotificationCompat.Builder(context, channelId)
        .SetSmallIcon(Android.Resource.Drawable.SymDefAppIcon)
        .SetContentTitle(notification.Title)
        .SetContentText(notification.Body)
        .SetPriority(NotificationCompat.PriorityDefault)
        .SetAutoCancel(true);

    var notificationManager = (NotificationManager) GetSystemService(NotificationService);
    notificationManager.Notify(123, builder.SetContentIntent(pendingIntent).Build());
};
```

You can find more specific instructions for android at the official [Firebase documentation](https://firebase.google.com/docs/cloud-messaging/android/client?hl=en)

## Usage

Take a look at the [documentation](https://github.com/xamarin/GoogleApisForiOSComponents/blob/master/docs/Firebase/CloudMessaging/GettingStarted.md) for the Xamarin.Firebase.iOS.CloudMessaging packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseCloudMessaging.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/CloudMessaging/IFirebaseCloudMessaging.cs)
- [sample/.../PushNotificationService.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/sample/Playground/Common/Services/PushNotification/PushNotificationService.cs)

## Test with curl

```
curl --location 'https://fcm.googleapis.com/fcm/send' \
--header 'Authorization: key=<your-api-token-from-firebase-console-cloud-messaging-project-settings>' \
--header 'Content-Type: application/json' \
--data '{
 "to" : "<your-device-fcm-token>",
 "collapse_key" : "type_a",
 "mutable_content": true,
 "notification" : {
     "title": "Knock knock",
     "body" : "Who's there?",
     "badge": 1
 },
 "data" : {
     "body" : "body of your notification in data",
     "title": "title of your notification in data",
     "is_silent_in_foreground": false
 }
}'
```

#### Extra flags
##### is_silent_in_foreground
- add `"is_silent_in_foreground": true` to the `data` payload to prevent showing the local push notification - as the name suggests, this only works when the app is in foreground otherwise the notifications will still be shown

## Troubleshooting

If you are having trouble receiving push notifications on your device, take a look at this helpful https://github.com/TobiasBuchholz/Plugin.Firebase/issues/145#issuecomment-1455182588 by @andyzukunft. Additionally he has created a dedicated project to simplify the demonstration on how Firebase Cloud Messaging works: https://github.com/andyzukunft/Plugin.Firebase/tree/fcm-demo/sample/Fcm

## Release notes
- Version 2.0.4
  - Add FirebaseCloudMessagingImplementation.ShowLocalNotificationAction (issue #163)
- Version 2.0.3
  - Enable silent push notifications when the app is in foreground (PR #188)
- Version 2.0.2
  - Prevent error message when no image is attached to push notifications
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net6.0 tfm

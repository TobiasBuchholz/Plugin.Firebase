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

#### Android `FirebaseMessagingService`
`Plugin.Firebase.CloudMessaging` includes a default Android `FirebaseMessagingService` that handles the `com.google.firebase.MESSAGING_EVENT` intent and raises the plugin notification/token events. The generated manifest registers this service with `android:exported="false"`, matching Firebase's documented setup for app message services.

Most apps should not add a second `FirebaseMessagingService`. Multiple services with the same FCM intent can produce duplicate notifications or make message handling depend on manifest merge details.

If your app needs full custom message handling, implement your own service and keep it non-exported:

```c#
[Service(Exported = false)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class PushNotificationFirebaseMessagingService : FirebaseMessagingService
{
    public override void OnMessageReceived(RemoteMessage message)
    {
        // Handle the message, or forward it to CrossFirebaseCloudMessaging.Current if desired.
    }
}
```

Then remove the plugin-provided generated service from your app manifest. Build once and inspect `obj/<Configuration>/net10.0-android/AndroidManifest.xml` to find the generated Java service name, then remove that service with manifest merge tooling:

```xml
<manifest
  xmlns:android="http://schemas.android.com/apk/res/android"
  xmlns:tools="http://schemas.android.com/tools">
  <application>
    <service
      android:name="crc64d2dcc2805d51c0a8.MyFirebaseMessagingService"
      tools:node="remove" />
  </application>
</manifest>
```

The `crc...` prefix is generated by .NET for Android and can change if the service namespace or assembly changes, so verify it from your build output when upgrading.

#### Customize local push notifications
Local push notifications are shown by default in a specific way, but you have the following two options to customize this behavior:

On Android, `FCMNotification.ChannelId` contains the raw notification channel ID specified by the sender. It is `null` when the sender omitted the channel, for data-only messages, and on iOS or other non-Android targets. The property does not validate that the channel exists and does not apply Firebase's manifest or default-channel fallback, so create every channel before using it and retain an app-configured fallback.

##### Overriding `FirebaseCloudMessagingImplementation.NotificationBuilderProvider`:

```c#
FirebaseCloudMessagingImplementation.NotificationBuilderProvider = notification => {
    var targetChannelId = notification.ChannelId ?? channelId;
    return new NotificationCompat.Builder(context, targetChannelId)
        .SetSmallIcon(Android.Resource.Drawable.SymDefAppIcon)
        .SetContentTitle(notification.Title)
        .SetContentText(notification.Body)
        .SetPriority(NotificationCompat.PriorityDefault)
        .SetAutoCancel(true);
};
```

##### Setting `FirebaseCloudMessagingImplementation.ShowLocalNotificationAction`:

```c#
FirebaseCloudMessagingImplementation.ShowLocalNotificationAction = notification => {

    var intent = PackageManager.GetLaunchIntentForPackage(PackageName);
    intent.PutExtra(FirebaseCloudMessagingImplementation.IntentKeyFCMNotification, notification.ToBundle());
    intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

    var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
    var targetChannelId = notification.ChannelId ?? channelId;
    var builder = new NotificationCompat.Builder(context, targetChannelId)
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

Take a look at the [documentation](https://github.com/AdamEssenmacher/GoogleApisForiOSComponents/blob/master/docs/Firebase/CloudMessaging/GettingStarted.md) for the AdamE.Firebase.iOS.CloudMessaging packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseCloudMessaging.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/CloudMessaging/IFirebaseCloudMessaging.cs)
- [sample/.../PushNotificationService.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/sample/Playground/Common/Services/PushNotification/PushNotificationService.cs)

On iOS only, `NotificationTapped` puts the native `UNNotificationResponse.ActionIdentifier` in `e.Notification.Data["ActionIdentifier"]`, both for an immediate tap and for a cold-start tap kept until the first subscriber is added, and it overwrites a sender-supplied `ActionIdentifier` data key.

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
     "is_silent_in_foreground": "false"
 }
}'
```

#### Extra flags
##### is_silent_in_foreground
Add `"is_silent_in_foreground": "true"` to the `data` payload to prevent showing the local push notification. The flag prevents a notification to be generated if the app is in foreground - otherwise the notifications will still be shown.  Event `CrossFirebaseCloudMessaging.Current.NotificationReceived` is still triggered to allow for manual notification handling.

The flag is read after the `NotificationReceived` subscribers return. On iOS, a subscriber can set it in `e.Notification.Data` to suppress the banner, and a silent notification completes the `willPresentNotification` delegate with `UNNotificationPresentationOptions.None`. Check `e.Notification.Data` for `null` before writing to it: it is `null` when the payload has no alert or `notification` dictionary, for example for local notifications, and such a notification can't be marked silent this way.

Note: this is a Plugin.Firebase custom field and hence not documented with google FCM documentation.

## Troubleshooting

If you are having trouble receiving push notifications on your device, take a look at this helpful https://github.com/TobiasBuchholz/Plugin.Firebase/issues/145#issuecomment-1455182588 by @andyzukunft. Additionally he has created a dedicated project to simplify the demonstration on how Firebase Cloud Messaging works: https://github.com/andyzukunft/Plugin.Firebase/tree/fcm-demo/sample/Fcm

## Next-release migration notes

On iOS, the `willPresentNotification` and `didReceiveNotificationResponse` delegates now call their completion handler exactly once on every path, as Apple requires:

- A foreground notification with `is_silent_in_foreground` set to `true` completes with `UNNotificationPresentationOptions.None`. Previously the completion handler was never called. The notification is still not shown.
- The presentation options are chosen after the `NotificationReceived` subscribers return, so a subscriber can set `is_silent_in_foreground` in `e.Notification.Data` to suppress the banner. Android already read the flag after its subscribers. `Data` is `null` for notifications without an alert or `notification` dictionary (for example local notifications), so check it before writing.
- If a `NotificationReceived` subscriber throws, or the notification can't be converted, iOS now completes with `Banner`, `List` and `Sound`, so the notification is shown unless it was marked silent before the throw. Previously the completion handler was skipped and nothing was shown. Android still skips its local notification when a subscriber throws.
- If a `NotificationTapped` subscriber throws, `didReceiveNotificationResponse` now still completes.
- In both delegates the exception still propagates out of the delegate method into the native caller, as before, so it remains unhandled in your app. Catch exceptions in your subscribers.

On iOS, every `NotificationTapped` notification now carries `Data["ActionIdentifier"]`:

- A tap that arrives before the first `NotificationTapped` subscriber, such as the tap that cold-starts the app, keeps its action identifier when it is raised for that subscriber. Previously only an immediate tap had it. On this path too, it overwrites a sender-supplied `ActionIdentifier` data key.
- A tapped notification has no `Data` when its payload has an `aps` dictionary without `alert`, or has neither an `aps` nor a `notification` dictionary (for example a local notification). It is now raised as a copy whose `Data` holds only `ActionIdentifier`. Previously an immediate tap of such a notification threw a `NullReferenceException`, and a kept tap was raised with `Data` set to `null`.

## Release notes
- Version 4.0.1
  - Add iOS action identifier to notification body (PR #547)
- Version 4.0.0
  - Upgrade baseline to **.NET 9+**.
- Version 3.1.2
  - Using AdamE.Firebase.iOS.* minimum version 11
- Version 3.1.1
  - Fix/iOS notifications not playing sound (PR #352)
- Version 3.1.0
  - Update to .net8
- Version 3.0.0
  - Swapped Xamarin.Firebase.iOS.CloudMessaging (native SDK 8.10.0) for AdamE.Firebase.iOS.CloudMessaging (native SDK 10.24.0)
- Version 2.0.4
  - Add FirebaseCloudMessagingImplementation.ShowLocalNotificationAction (issue #163)
- Version 2.0.3
  - Enable silent push notifications when the app is in foreground (PR #188)
- Version 2.0.2
  - Prevent error message when no image is attached to push notifications
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net6.0 tfm

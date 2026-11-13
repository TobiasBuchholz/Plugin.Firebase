using Firebase.Messaging;
using Plugin.Firebase.CloudMessaging.Platforms.Android.Extensions;

namespace Plugin.Firebase.CloudMessaging.Platforms.Android;

[Service(Exported = false)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class MyFirebaseMessagingService : FirebaseMessagingService
{
    public override void OnMessageReceived(RemoteMessage message)
    {
        base.OnMessageReceived(message);
        CrossFirebaseCloudMessaging.Current.OnNotificationReceived(message.ToFCMNotification());
    }

#pragma warning disable CS0672 // Keep the callback available for apps using token mode.
    public override async void OnNewToken(string token)
#pragma warning restore CS0672
    {
        await CrossFirebaseCloudMessaging.Current.OnTokenRefreshAsync();
    }

    public override void OnRegistered(string installationId)
    {
        ((FirebaseCloudMessagingImplementation) CrossFirebaseCloudMessaging.Current).OnRegistered(installationId);
    }

    public override void OnUnregistered(string installationId)
    {
        ((FirebaseCloudMessagingImplementation) CrossFirebaseCloudMessaging.Current).OnUnregistered(installationId);
    }
}
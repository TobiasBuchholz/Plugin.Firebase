using System;
using Android.App;
using Firebase.Messaging;
using Plugin.Firebase.CloudMessaging;

namespace Plugin.Firebase.Android.CloudMessaging
{
    [Service]
    [IntentFilter(new [] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            FirebaseCloudMessagingImplementation.Instance.OnNotificationReceived(message.ToFCMNotification());
        }

        public override async void OnNewToken(string token)
        {
            await FirebaseCloudMessagingImplementation.Instance.OnTokenRefreshAsync();
        }
    }
}

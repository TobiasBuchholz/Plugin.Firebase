using System;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Support.V4.App;
using Firebase.Iid;
using Plugin.Firebase.Abstractions.CloudMessaging;
using Plugin.Firebase.Abstractions.CloudMessaging.EventArgs;
using Plugin.Firebase.Abstractions.Common;
using Plugin.Firebase.Android;
using Plugin.Firebase.Android.Extensions;

namespace Plugin.Firebase.CloudMessaging
{
    public sealed class FirebaseCloudMessagingImplementation : DisposableBase, IFirebaseCloudMessaging
    {
        private const string IntentKeyFCMNotification = "intent_key_fcm_notification";
        private static Context _context;

        public FirebaseCloudMessagingImplementation()
        {
            _context = Application.Context;
        }

        public void CheckIfValid()
        {
            var resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(_context);
            if (resultCode == ConnectionResult.Success) {
                OnTokenRefresh();
            } else {
                Error?.Invoke(this, new FCMErrorEventArgs(GetErrorMessage(resultCode)));
            }
        }

        private static string GetErrorMessage(int resultCode)
        {
            return GoogleApiAvailability.Instance.IsUserResolvableError(resultCode)
                ? GoogleApiAvailability.Instance.GetErrorString(resultCode)
                : "This device is not supported";
        }

        public void OnTokenRefresh()
        {
            TokenChanged?.Invoke(this, new FCMTokenChangedEventArgs(FirebaseInstanceId.Instance.Token));
        }

        public void OnNotificationReceived(FCMNotification fcmNotification)
        {
            NotificationReceived?.Invoke(this, new FCMNotificationReceivedEventArgs(fcmNotification));
            HandleShowLocalNotification(fcmNotification);
        }

        private static void HandleShowLocalNotification(FCMNotification notification)
        {
            if(ShowLocalNotificationAction == null) {
                ShowLocalNotification(notification);
            } else {
                ShowLocalNotificationAction(notification);
            }
        }

        private static void ShowLocalNotification(FCMNotification notification)
        {
            var intent = _context.PackageManager.GetLaunchIntentForPackage(_context.PackageName);
            intent.PutExtra(IntentKeyFCMNotification, notification.ToBundle());
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            var pendingIntent = PendingIntent.GetActivity(_context, 0, intent, 0);

            var builder = new NotificationCompat.Builder(_context, ChannelId)
                .SetSmallIcon(Resource.Drawable.ic_stat_notify)
                .SetContentTitle(notification.Title)
                .SetContentText(notification.Body)
                .SetPriority(NotificationCompat.PriorityDefault)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);

            var notificationManager = (NotificationManager) _context.GetSystemService(Context.NotificationService);
            notificationManager.Notify(1337, builder.Build());
        }
        
        public void OnNewIntent(Intent intent)
        {
            if(intent.HasExtra(IntentKeyFCMNotification)) {
                var notification = intent.GetBundleExtra(IntentKeyFCMNotification).ToFCMNotification();
                NotificationTapped?.Invoke(this, new FCMNotificationTappedEventArgs(notification));
                intent.RemoveExtra(IntentKeyFCMNotification);
            }
        }

        public static string ChannelId { get; set; }
        public static Action<FCMNotification> ShowLocalNotificationAction { private get; set; }
        
        public event EventHandler<FCMTokenChangedEventArgs> TokenChanged;
        public event EventHandler<FCMNotificationReceivedEventArgs> NotificationReceived;
        public event EventHandler<FCMNotificationTappedEventArgs> NotificationTapped;
        public event EventHandler<FCMErrorEventArgs> Error;
        public string Token => FirebaseInstanceId.Instance.Token;
    }
}
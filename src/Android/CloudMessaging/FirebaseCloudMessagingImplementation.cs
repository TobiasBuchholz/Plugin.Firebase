using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using AndroidX.Core.App;
using Firebase.Iid;
using Firebase.Messaging;
using Plugin.Firebase.Android.CloudMessaging;
using Plugin.Firebase.CloudMessaging.EventArgs;
using Plugin.Firebase.Common;
using Plugin.Firebase.Extensions;

 namespace Plugin.Firebase.CloudMessaging
{
    public sealed class FirebaseCloudMessagingImplementation : DisposableBase, IFirebaseCloudMessaging
    {
        private const string IntentKeyFCMNotification = "intent_key_fcm_notification";
        private static Context _context;
        private static FirebaseCloudMessagingImplementation _instance;

        public static FirebaseCloudMessagingImplementation Instance {
            get {
                if(_instance == null) {
                    throw new FirebaseException($"Make sure to call {nameof(FirebaseCloudMessagingImplementation)}.Initialize() before accessing it's Instance");
                }
                return _instance;
            }
        }
        
        public static string ChannelId { get; set; }
        public static Action<FCMNotification> ShowLocalNotificationAction { private get; set; }

        public static void Initialize()
        {
            if(_instance == null) {
                _instance = new FirebaseCloudMessagingImplementation();
            }
        }
        
        private FCMNotification _missedTappedNotification;

        public FirebaseCloudMessagingImplementation()
        {
            _context = Application.Context;
        }

        public async Task CheckIfValidAsync()
        {
            var resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(_context);
            if (resultCode == ConnectionResult.Success) {
                await OnTokenRefreshAsync();
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

        public async Task OnTokenRefreshAsync()
        {
            TokenChanged?.Invoke(this, new FCMTokenChangedEventArgs(await GetTokenAsync()));
        }

        public void OnNotificationReceived(FCMNotification fcmNotification)
        {
            NotificationReceived?.Invoke(this, new FCMNotificationReceivedEventArgs(fcmNotification));
            HandleShowLocalNotificationIfNeeded(fcmNotification);
        }

        private static void HandleShowLocalNotificationIfNeeded(FCMNotification fcmNotification)
        {
            if(!string.IsNullOrEmpty(fcmNotification.Title)) {
                HandleShowLocalNotification(fcmNotification);
            }
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
            if(intent.Extras != null) {
                HandleNotificationFromIntent(intent);
            }
        }

        private void HandleNotificationFromIntent(Intent intent)
        {
            var notification = intent.GetNotificationFromExtras(IntentKeyFCMNotification);
            if(_notificationTapped == null) {
                _missedTappedNotification = notification;
            } else {
                _notificationTapped.Invoke(this, new FCMNotificationTappedEventArgs(notification));
            }
            intent.RemoveExtra(IntentKeyFCMNotification);
        }

        public async Task<string> GetTokenAsync()
        {
            var result = await FirebaseInstanceId.Instance.GetInstanceId().ToTask<IInstanceIdResult>();
            return result.Token;
        }

        public Task SubscribeToTopicAsync(string topic)
        {
            return FirebaseMessaging.Instance.SubscribeToTopic(topic).ToTask();
        }

        public Task UnsubscribeFromTopicAsync(string topic)
        {
            return FirebaseMessaging.Instance.UnsubscribeFromTopic(topic).ToTask();
        }

        public event EventHandler<FCMTokenChangedEventArgs> TokenChanged;
        public event EventHandler<FCMNotificationReceivedEventArgs> NotificationReceived;
        public event EventHandler<FCMErrorEventArgs> Error;
        

        private event EventHandler<FCMNotificationTappedEventArgs> _notificationTapped;
        public event EventHandler<FCMNotificationTappedEventArgs> NotificationTapped {
            add {
                _notificationTapped += value;
                if(_missedTappedNotification != null) {
                    _notificationTapped?.Invoke(this, new FCMNotificationTappedEventArgs(_missedTappedNotification));
                    _missedTappedNotification = null;
                }
            }
            remove => _notificationTapped -= value;
        }
    }
}
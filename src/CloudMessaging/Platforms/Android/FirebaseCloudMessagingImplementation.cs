using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Extensions;
using AndroidX.Core.App;
using Firebase;
using Firebase.Messaging;
using Plugin.Firebase.CloudMessaging.EventArgs;
using Plugin.Firebase.CloudMessaging.Platforms.Android.Extensions;
using Plugin.Firebase.Core;
using Application = Android.App.Application;

namespace Plugin.Firebase.CloudMessaging;

public sealed class FirebaseCloudMessagingImplementation : DisposableBase, IFirebaseCloudMessaging
{
    private static Context _context;

    public static string IntentKeyFCMNotification { get; set; } = "intent_key_fcm_notification";
    public static string ChannelId { get; set; }
    public static int SmallIconRef { private get; set; } = Android.Resource.Drawable.SymDefAppIcon;
    public static Func<FCMNotification, NotificationCompat.Builder> NotificationBuilderProvider { private get; set; }
#nullable enable
    public static Action<FCMNotification>? ShowLocalNotificationAction { private get; set; }
#nullable restore

    private FCMNotification _missedTappedNotification;

    public FirebaseCloudMessagingImplementation()
    {
        _context = Application.Context;
    }

    public async Task CheckIfValidAsync()
    {
        var resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(_context);
        if(resultCode == ConnectionResult.Success) {
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
        var packageManager = _context.PackageManager;
        var applicationInfo = OperatingSystem.IsAndroidVersionAtLeast(33)
            ? packageManager.GetApplicationInfo(
                _context.PackageName, PackageManager.ApplicationInfoFlags.Of((long) PackageInfoFlags.MetaData))
            : packageManager.GetApplicationInfo(_context.PackageName, PackageInfoFlags.MetaData);
        if(applicationInfo.MetaData?.GetBoolean("firebase_messaging_installation_id_enabled") == true) {
            return;
        }

        TokenChanged?.Invoke(this, new FCMTokenChangedEventArgs(await GetTokenAsync()));
    }

    internal void OnRegistered(string installationId)
    {
        RegistrationReceived?.Invoke(this, new FCMRegistrationReceivedEventArgs(installationId));
    }

    internal void OnUnregistered(string installationId)
    {
        UnregistrationReceived?.Invoke(this, new FCMUnregistrationReceivedEventArgs(installationId));
    }

    public void OnNotificationReceived(FCMNotification fcmNotification)
    {
        NotificationReceived?.Invoke(this, new FCMNotificationReceivedEventArgs(fcmNotification));
        TryHandleShowLocalNotificationIfNeeded(fcmNotification);
    }

    private static void TryHandleShowLocalNotificationIfNeeded(FCMNotification fcmNotification)
    {
        try {
            HandleShowLocalNotificationIfNeeded(fcmNotification);
        } catch(Exception e) {
            Console.WriteLine($"[Plugin.Firebase]: Couldn't show local push notification: {e.Message}");
        }
    }

    private static void HandleShowLocalNotificationIfNeeded(FCMNotification fcmNotification)
    {
        if(!fcmNotification.IsSilentInForeground) {
            var showLocalNotificationAction = ShowLocalNotificationAction;
            if(showLocalNotificationAction == null) {
                HandleShowLocalNotification(fcmNotification);
            } else {
                showLocalNotificationAction(fcmNotification);
            }
        }
    }

    private static void HandleShowLocalNotification(FCMNotification notification)
    {
        var intent = _context.PackageManager.GetLaunchIntentForPackage(_context.PackageName);
        intent.PutExtra(IntentKeyFCMNotification, notification.ToBundle());
        intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        var pendingIntent = PendingIntent.GetActivity(_context, 0, intent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
        var builder = NotificationBuilderProvider?.Invoke(notification) ?? CreateDefaultNotificationBuilder(notification);
        var notificationManager = (NotificationManager) _context.GetSystemService(Context.NotificationService);
        notificationManager.Notify(1337, builder.SetContentIntent(pendingIntent).Build());
    }

    private static NotificationCompat.Builder CreateDefaultNotificationBuilder(FCMNotification notification)
    {
        return new NotificationCompat.Builder(_context, ChannelId)
            .SetSmallIcon(SmallIconRef)
            .TrySetBigPictureStyle(notification)
            .SetContentTitle(notification.Title)
            .SetContentText(notification.Body)
            .SetPriority(NotificationCompat.PriorityDefault)
            .SetAutoCancel(true);
    }

    public static void OnNewIntent(Intent intent)
    {
        if(intent.IsNotificationTappedIntent(IntentKeyFCMNotification)) {
            ((FirebaseCloudMessagingImplementation) CrossFirebaseCloudMessaging.Current).HandleNotificationFromIntent(intent);
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
#pragma warning disable CS0618 // Keep FCM token retrieval available for apps using token mode.
        var token = (await FirebaseMessaging.Instance.GetToken()).ToString();
#pragma warning restore CS0618
        return string.IsNullOrEmpty(token) ? throw new FirebaseException("Couldn't retrieve FCM token") : token;
    }

    public async Task RegisterAsync()
    {
        await FirebaseMessaging.Instance.Register().AsAsync();
    }

    public async Task UnregisterAsync()
    {
        await FirebaseMessaging.Instance.Unregister().AsAsync();
    }

    public Task SubscribeToTopicAsync(string topic)
    {
        return FirebaseMessaging.Instance.SubscribeToTopic(topic).AsAsync();
    }

    public Task UnsubscribeFromTopicAsync(string topic)
    {
        return FirebaseMessaging.Instance.UnsubscribeFromTopic(topic).AsAsync();
    }

    public event EventHandler<FCMTokenChangedEventArgs> TokenChanged;
    public event EventHandler<FCMRegistrationReceivedEventArgs> RegistrationReceived;
    public event EventHandler<FCMUnregistrationReceivedEventArgs> UnregistrationReceived;
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
using Firebase.CloudMessaging;
using Foundation;
using Plugin.Firebase.CloudMessaging.EventArgs;
using Plugin.Firebase.Common;
using Plugin.Firebase.iOS.CloudMessaging;
using UIKit;
using UserNotifications;

namespace Plugin.Firebase.CloudMessaging;

[Preserve(AllMembers = true)]
public sealed class FirebaseCloudMessagingImplementation : NSObject, IFirebaseCloudMessaging, IUNUserNotificationCenterDelegate, IMessagingDelegate
{
    private FCMNotification _missedTappedNotification;

    public static void Initialize()
    {
        var instance = (FirebaseCloudMessagingImplementation) CrossFirebaseCloudMessaging.Current;
        instance.RegisterForRemoteNotifications();
        instance.OnTokenRefreshAsync();
    }

    private void RegisterForRemoteNotifications()
    {
        if(UIDevice.CurrentDevice.CheckSystemVersion(10, 0)) {
            UNUserNotificationCenter.Current.Delegate = this;
            Messaging.SharedInstance.Delegate = this;
            UIApplication.SharedApplication.RegisterForRemoteNotifications();
        } else {
            var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
            var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
            UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
        }
    }

    public Task OnTokenRefreshAsync()
    {
        TokenChanged?.Invoke(this, new FCMTokenChangedEventArgs(Messaging.SharedInstance.FcmToken));
        return Task.CompletedTask;
    }

    public Task CheckIfValidAsync()
    {
        if(UIDevice.CurrentDevice.CheckSystemVersion(10, 0)) {
            RequestAuthorization();
        }
        return Task.CompletedTask;
    }

    private void RequestAuthorization()
    {
        UNUserNotificationCenter.Current.RequestAuthorization(
            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
            (granted, error) => {
                if(!granted) {
                    Error?.Invoke(this, new FCMErrorEventArgs("User permission for remote notifications is not granted"));
                }
            });
    }

    [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
    public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
    {
        OnNotificationReceived(notification.ToFCMNotification());
        completionHandler(UNNotificationPresentationOptions.Alert);
    }

    public void OnNotificationReceived(FCMNotification message)
    {
        NotificationReceived?.Invoke(this, new FCMNotificationReceivedEventArgs(message));
    }

    [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
    public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
    {
        if(_notificationTapped == null) {
            _missedTappedNotification = response.Notification.ToFCMNotification();
        } else {
            _notificationTapped.Invoke(this, new FCMNotificationTappedEventArgs(response.Notification.ToFCMNotification()));
        }

        completionHandler();
    }

    public Task<string> GetTokenAsync()
    {
        var token = Messaging.SharedInstance.FcmToken;
        return string.IsNullOrEmpty(token) ? throw new FirebaseException("Couldn't retrieve FCM token") : Task.FromResult(token);
    }

    public Task SubscribeToTopicAsync(string topic)
    {
        return Messaging.SharedInstance.SubscribeAsync(topic);
    }

    public Task UnsubscribeFromTopicAsync(string topic)
    {
        return Messaging.SharedInstance.UnsubscribeAsync(topic);
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
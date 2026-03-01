using Firebase.CloudMessaging;
using Plugin.Firebase.CloudMessaging.EventArgs;
using Plugin.Firebase.CloudMessaging.Platforms.iOS.Extensions;
using Plugin.Firebase.Core.Exceptions;
using UserNotifications;

namespace Plugin.Firebase.CloudMessaging;

/// <summary>
/// iOS implementation of <see cref="IFirebaseCloudMessaging"/> that wraps the native Firebase Cloud Messaging SDK.
/// </summary>
[Preserve(AllMembers = true)]
public sealed class FirebaseCloudMessagingImplementation
    : NSObject,
        IFirebaseCloudMessaging,
        IUNUserNotificationCenterDelegate,
        IMessagingDelegate
{
    private FCMNotification _missedTappedNotification;

    /// <summary>
    /// Initializes the Firebase Cloud Messaging service and registers for remote notifications.
    /// </summary>
    public static void Initialize(bool skipApnsDeviceTokenRequest = false)
    {
        var instance = (FirebaseCloudMessagingImplementation) CrossFirebaseCloudMessaging.Current;
        instance.RegisterForRemoteNotifications(skipApnsDeviceTokenRequest);
        if(!skipApnsDeviceTokenRequest) {
            instance.OnTokenRefreshAsync();
        }
    }

    private void RegisterForRemoteNotifications(bool skipApnsDeviceTokenRequest)
    {
        if(UIDevice.CurrentDevice.CheckSystemVersion(10, 0)) {
            UNUserNotificationCenter.Current.Delegate = this;
            Messaging.SharedInstance.Delegate = this;
            if(!skipApnsDeviceTokenRequest) {
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            }
        } else {
            var allNotificationTypes =
                UIUserNotificationType.Alert
                | UIUserNotificationType.Badge
                | UIUserNotificationType.Sound;
            var settings = UIUserNotificationSettings.GetSettingsForTypes(
                allNotificationTypes,
                null
            );
            UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
        }
    }

    public static void RequestApnsDeviceToken()
    {
        UIApplication.SharedApplication.RegisterForRemoteNotifications();
        var instance = (FirebaseCloudMessagingImplementation) CrossFirebaseCloudMessaging.Current;
        instance.OnTokenRefreshAsync();
    }

    /// <summary>
    /// Triggers a token refresh check and raises the <see cref="TokenChanged"/> event if a token is available.
    /// </summary>
    /// <returns>A completed task.</returns>
    public Task OnTokenRefreshAsync()
    {
        OnTokenRefresh(Messaging.SharedInstance.FcmToken);
        return Task.CompletedTask;
    }

    private void OnTokenRefresh(string fcmToken)
    {
        TokenChanged?.Invoke(this, new FCMTokenChangedEventArgs(fcmToken));
    }

    /// <inheritdoc/>
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
            UNAuthorizationOptions.Alert
                | UNAuthorizationOptions.Badge
                | UNAuthorizationOptions.Sound,
            (granted, _) => {
                if(!granted) {
                    Error?.Invoke(
                        this,
                        new FCMErrorEventArgs(
                            "User permission for remote notifications is not granted"
                        )
                    );
                }
            }
        );
    }

    /// <summary>
    /// Handles notification presentation when the app is in the foreground.
    /// </summary>
    /// <param name="center">The notification center.</param>
    /// <param name="notification">The notification being presented.</param>
    /// <param name="completionHandler">The completion handler to call with presentation options.</param>
    [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
    public void WillPresentNotification(
        UNUserNotificationCenter center,
        UNNotification notification,
        Action<UNNotificationPresentationOptions> completionHandler
    )
    {
        var fcmNotification = notification.ToFCMNotification();
        OnNotificationReceived(fcmNotification);
        if(!fcmNotification.IsSilentInForeground) {
            if(OperatingSystem.IsIOSVersionAtLeast(14)) {
                completionHandler(
                    UNNotificationPresentationOptions.Banner
                        | UNNotificationPresentationOptions.List
                        | UNNotificationPresentationOptions.Sound
                );
            } else {
                completionHandler(
                    UNNotificationPresentationOptions.Alert
                        | UNNotificationPresentationOptions.Sound
                );
            }
        }
    }

    /// <summary>
    /// Raises the <see cref="NotificationReceived"/> event with the specified notification.
    /// </summary>
    /// <param name="message">The received FCM notification.</param>
    public void OnNotificationReceived(FCMNotification message)
    {
        NotificationReceived?.Invoke(this, new FCMNotificationReceivedEventArgs(message));
    }

    /// <summary>
    /// Handles the user's response to a notification (e.g., tapping on it).
    /// </summary>
    /// <param name="center">The notification center.</param>
    /// <param name="response">The user's response to the notification.</param>
    /// <param name="completionHandler">The completion handler to call when processing is complete.</param>
    [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
    public void DidReceiveNotificationResponse(
        UNUserNotificationCenter center,
        UNNotificationResponse response,
        Action completionHandler
    )
    {
        if(_notificationTapped == null) {
            _missedTappedNotification = response.Notification.ToFCMNotification();
        } else {
            var notification = response.Notification.ToFCMNotification();
            var actionId = response.ActionIdentifier?.ToString();
            if(!string.IsNullOrEmpty(actionId)) {
                notification.Data[nameof(response.ActionIdentifier)] = actionId;
            }
            _notificationTapped.Invoke(this, new FCMNotificationTappedEventArgs(notification));
        }

        completionHandler();
    }

    /// <summary>
    /// Called when a new FCM registration token is received.
    /// </summary>
    /// <param name="messaging">The messaging instance.</param>
    /// <param name="fcmToken">The new FCM token.</param>
    [Export("messaging:didReceiveRegistrationToken:")]
    public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
    {
        OnTokenRefresh(fcmToken);
    }

    /// <inheritdoc/>
    public Task<string> GetTokenAsync()
    {
        var token = Messaging.SharedInstance.FcmToken;
        return string.IsNullOrEmpty(token)
            ? throw new FirebaseException("Couldn't retrieve FCM token")
            : Task.FromResult(token);
    }

    /// <inheritdoc/>
    public Task SubscribeToTopicAsync(string topic)
    {
        return Messaging.SharedInstance.SubscribeAsync(topic);
    }

    /// <inheritdoc/>
    public Task UnsubscribeFromTopicAsync(string topic)
    {
        return Messaging.SharedInstance.UnsubscribeAsync(topic);
    }

    /// <inheritdoc/>
    public event EventHandler<FCMTokenChangedEventArgs> TokenChanged;

    /// <inheritdoc/>
    public event EventHandler<FCMNotificationReceivedEventArgs> NotificationReceived;

    /// <inheritdoc/>
    public event EventHandler<FCMErrorEventArgs> Error;

    private event EventHandler<FCMNotificationTappedEventArgs> _notificationTapped;

    /// <inheritdoc/>
    public event EventHandler<FCMNotificationTappedEventArgs> NotificationTapped {
        add {
            _notificationTapped += value;
            if(_missedTappedNotification != null) {
                _notificationTapped?.Invoke(
                    this,
                    new FCMNotificationTappedEventArgs(_missedTappedNotification)
                );
                _missedTappedNotification = null;
            }
        }
        remove => _notificationTapped -= value;
    }
}
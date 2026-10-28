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
    /// <remarks>
    /// Raises <see cref="NotificationReceived"/>, then calls <paramref name="completionHandler"/> exactly once.
    /// The presentation options are chosen after the subscribers return, so a subscriber can still set
    /// <c>is_silent_in_foreground</c> in <see cref="FCMNotification.Data"/>. Check <see cref="FCMNotification.Data"/>
    /// for <see langword="null"/> first: it is null when the payload has no alert or <c>notification</c> dictionary,
    /// for example for local notifications, and such a notification can't be marked silent this way.
    /// A silent notification completes with <see cref="UNNotificationPresentationOptions.None"/>; any other
    /// notification completes with <see cref="UNNotificationPresentationOptions.Banner"/>,
    /// <see cref="UNNotificationPresentationOptions.List"/> and <see cref="UNNotificationPresentationOptions.Sound"/>.
    /// If a subscriber or the conversion throws, the completion handler is still called with these options, so the
    /// notification is shown unless it was marked silent before the throw. The exception then propagates out of
    /// this method.
    /// </remarks>
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
        CompleteForegroundNotification(() => notification.ToFCMNotification(), completionHandler);
    }

    internal void CompleteForegroundNotification(
        Func<FCMNotification> getNotification,
        Action<UNNotificationPresentationOptions> completionHandler
    )
    {
        FCMNotification fcmNotification = null;
        try {
            fcmNotification = getNotification();
            OnNotificationReceived(fcmNotification);
        }
        finally {
            // the delegate contract requires completing exactly once on every path; reading the
            // options here lets subscribers mark the notification silent before it is presented
            completionHandler(GetPresentationOptions(fcmNotification));
        }
    }

    private static UNNotificationPresentationOptions GetPresentationOptions(
        FCMNotification notification
    )
    {
        return notification?.IsSilentInForeground == true
            ? UNNotificationPresentationOptions.None
            : UNNotificationPresentationOptions.Banner
                | UNNotificationPresentationOptions.List
                | UNNotificationPresentationOptions.Sound;
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
    /// <remarks>
    /// Raises <see cref="NotificationTapped"/>, or keeps the notification until the first subscriber is added,
    /// then calls <paramref name="completionHandler"/> exactly once. If a subscriber throws, the completion
    /// handler is still called and the exception then propagates out of this method.
    /// </remarks>
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
        CompleteNotificationResponse(
            () => response.Notification.ToFCMNotification(),
            () => response.ActionIdentifier?.ToString(),
            completionHandler
        );
    }

    internal void CompleteNotificationResponse(
        Func<FCMNotification> getNotification,
        Func<string> getActionIdentifier,
        Action completionHandler
    )
    {
        try {
            if(_notificationTapped == null) {
                _missedTappedNotification = getNotification();
            } else {
                var notification = getNotification();
                var actionIdentifier = getActionIdentifier();
                if(!string.IsNullOrEmpty(actionIdentifier)) {
                    notification.Data[nameof(UNNotificationResponse.ActionIdentifier)] = actionIdentifier;
                }
                _notificationTapped.Invoke(this, new FCMNotificationTappedEventArgs(notification));
            }
        }
        finally {
            // the delegate contract requires completing exactly once, even if a subscriber throws
            completionHandler();
        }
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
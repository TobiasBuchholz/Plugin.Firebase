using System;
using System.Threading.Tasks;
using Firebase.CloudMessaging;
using Firebase.InstanceID;
using Foundation;
using Plugin.Firebase.CloudMessaging.EventArgs;
using Plugin.Firebase.Common;
using Plugin.Firebase.iOS.CloudMessaging;
using UIKit;
using UserNotifications;

namespace Plugin.Firebase.CloudMessaging
{
    public sealed class FirebaseCloudMessagingImplementation : NSObject, IFirebaseCloudMessaging, IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        private static FirebaseCloudMessagingImplementation _instance;

        public static FirebaseCloudMessagingImplementation Instance {
            get {
                if(_instance == null) {
                    throw new FirebaseException($"Make sure to call {nameof(FirebaseCloudMessagingImplementation)}.Initialize() before accessing it's Instance");
                }
                return _instance;
            }
        }

        public static void Initialize()
        {
            if(_instance == null) {
                _instance = new FirebaseCloudMessagingImplementation();
                _instance.RegisterForRemoteNotifications();
                InstanceId.Notifications.ObserveTokenRefresh((sender, e) => _instance.OnTokenRefreshAsync());
                _instance.OnTokenRefreshAsync();
            }
        }
        
        private FCMNotification _missedTappedNotification;
        
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
        }

        // from the docs but never called actually
        public void ApplicationReceivedRemoteMessage(RemoteMessage remoteMessage)
        {
            OnNotificationReceived(remoteMessage.AppData.ToFCMNotification());
        }

        public Task<string> GetTokenAsync()
        {
            return Task.FromResult(Messaging.SharedInstance.FcmToken);
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
}
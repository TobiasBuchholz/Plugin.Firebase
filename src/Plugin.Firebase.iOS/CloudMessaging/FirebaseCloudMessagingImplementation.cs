using System;
using Firebase.CloudMessaging;
using Firebase.InstanceID;
using Foundation;
using Plugin.Firebase.Abstractions.CloudMessaging;
using Plugin.Firebase.Abstractions.CloudMessaging.EventArgs;
using Plugin.Firebase.iOS.Extensions;
using UIKit;
using UserNotifications;

namespace Plugin.Firebase.CloudMessaging
{
    public sealed class FirebaseCloudMessagingImplementation : NSObject, IFirebaseCloudMessaging, IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        public FirebaseCloudMessagingImplementation()
        {
            Initialize();
        }

        private void Initialize()
        {
            RegisterForRemoteNotifications();
            Messaging.SharedInstance.ShouldEstablishDirectChannel = true;
            InstanceId.Notifications.ObserveTokenRefresh((sender, e) => OnTokenRefresh());
            OnTokenRefresh();
        }

        public void OnTokenRefresh()
        {
            TokenChanged?.Invoke(this, new FCMTokenChangedEventArgs(InstanceId.SharedInstance.Token));
        }
        
        public void CheckIfValid()
        {
            if(UIDevice.CurrentDevice.CheckSystemVersion(10, 0)) {
                RequestAuthorization();
            }
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
        
        public void DidRefreshRegistrationToken(Messaging messaging, string fcmToken)
        {
            OnTokenRefresh();
        }

        [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            OnNotificationReceived(notification.ToFCMNotification());
        }
        
        public void OnNotificationReceived(FCMNotification message)
        {
            NotificationReceived?.Invoke(this, new FCMNotificationReceivedEventArgs(message));
        }

        [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
        public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            OnNotificationReceived(response.Notification.ToFCMNotification());
        }

        // from the docs but never called actually
        public void ApplicationReceivedRemoteMessage(RemoteMessage remoteMessage)
        {
            OnNotificationReceived(remoteMessage.AppData.ToFCMNotification());
        }
        
        public event EventHandler<FCMTokenChangedEventArgs> TokenChanged;
        public event EventHandler<FCMNotificationReceivedEventArgs> NotificationReceived;
        public event EventHandler<FCMErrorEventArgs> Error;
    }
}
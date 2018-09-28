using System;
using Plugin.Firebase.Abstractions.CloudMessaging.EventArgs;

namespace Plugin.Firebase.Abstractions.CloudMessaging
{
    public interface IFirebaseCloudMessaging : IDisposable
    {
        void CheckIfValid();
        void OnTokenRefresh();
        void OnNotificationReceived(FCMNotification fcmNotification);
        
        event EventHandler<FCMTokenChangedEventArgs> TokenChanged;
        event EventHandler<FCMNotificationReceivedEventArgs> NotificationReceived;
        event EventHandler<FCMErrorEventArgs> Error;

        string Token { get; }
    }
}
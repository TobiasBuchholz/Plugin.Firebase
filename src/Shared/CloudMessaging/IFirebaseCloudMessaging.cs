using System;
using System.Threading.Tasks;
using Plugin.Firebase.CloudMessaging.EventArgs;

namespace Plugin.Firebase.CloudMessaging
{
    public interface IFirebaseCloudMessaging : IDisposable
    {
        Task CheckIfValidAsync();
        Task OnTokenRefreshAsync();
        Task<string> GetTokenAsync();
        Task SubscribeToTopicAsync(string topic);
        Task UnsubscribeFromTopicAsync(string topic);
        void OnNotificationReceived(FCMNotification fcmNotification);
        
        event EventHandler<FCMTokenChangedEventArgs> TokenChanged;
        event EventHandler<FCMNotificationReceivedEventArgs> NotificationReceived;
        event EventHandler<FCMNotificationTappedEventArgs> NotificationTapped;
        event EventHandler<FCMErrorEventArgs> Error;
    }
}
using Firebase.Messaging;
using Plugin.Firebase.Abstractions.CloudMessaging;

namespace Plugin.Firebase.Android.Extensions
{
    public static class FCMNotificationExtensions
    {
        public static FCMNotification ToFCMNotification(this RemoteMessage message)
        {
            var notification = message.GetNotification();
            return notification == null ? FCMNotification.Empty() : new FCMNotification(notification.Body, notification.Title, message.Data);
        }
    }
}
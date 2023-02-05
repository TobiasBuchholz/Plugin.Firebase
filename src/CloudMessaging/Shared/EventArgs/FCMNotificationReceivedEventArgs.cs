namespace Plugin.Firebase.CloudMessaging.EventArgs
{
    public sealed class FCMNotificationReceivedEventArgs : System.EventArgs
    {
        public FCMNotificationReceivedEventArgs(FCMNotification notification)
        {
            Notification = notification;
        }

        public FCMNotification Notification { get; }
    }
}
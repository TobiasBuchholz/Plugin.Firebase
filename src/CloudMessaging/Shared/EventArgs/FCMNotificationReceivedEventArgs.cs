namespace Plugin.Firebase.CloudMessaging.EventArgs
{
    /// <summary>
    /// Event arguments for when a Firebase Cloud Messaging notification is received.
    /// </summary>
    public sealed class FCMNotificationReceivedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Creates a new <c>FCMNotificationReceivedEventArgs</c> instance.
        /// </summary>
        /// <param name="notification">The received notification.</param>
        public FCMNotificationReceivedEventArgs(FCMNotification notification)
        {
            Notification = notification;
        }

        /// <summary>
        /// Gets the received notification.
        /// </summary>
        public FCMNotification Notification { get; }
    }
}
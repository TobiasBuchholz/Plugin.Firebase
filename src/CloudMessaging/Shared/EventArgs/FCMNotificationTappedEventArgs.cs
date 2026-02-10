namespace Plugin.Firebase.CloudMessaging.EventArgs
{
    /// <summary>
    /// Event arguments for when a Firebase Cloud Messaging notification is tapped by the user.
    /// </summary>
    public sealed class FCMNotificationTappedEventArgs
    {
        /// <summary>
        /// Creates a new <c>FCMNotificationTappedEventArgs</c> instance.
        /// </summary>
        /// <param name="notification">The tapped notification.</param>
        public FCMNotificationTappedEventArgs(FCMNotification notification)
        {
            Notification = notification;
        }

        /// <summary>
        /// Gets the tapped notification.
        /// </summary>
        public FCMNotification Notification { get; }
    }
}
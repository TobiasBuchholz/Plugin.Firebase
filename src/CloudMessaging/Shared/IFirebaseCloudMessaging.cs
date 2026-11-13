using Plugin.Firebase.CloudMessaging.EventArgs;

namespace Plugin.Firebase.CloudMessaging
{
    /// <summary>
    /// Firebase Cloud Messaging delivers messages to app instances registered by token or Firebase Installation ID.
    /// </summary>
    public interface IFirebaseCloudMessaging : IDisposable
    {
        /// <summary>
        /// Checks whether device is able to receive cloud messages and if the user has granted the permissions for it.
        /// </summary>
        Task CheckIfValidAsync();

        /// <summary>
        /// Call it when the token was refreshed.
        /// </summary>
        /// <returns></returns>
        Task OnTokenRefreshAsync();

        /// <summary>
        /// Asynchronously gets the default FCM registration token. This creates a Firebase Installations ID, if one does not exist,
        /// and sends information about the application and the device to the Firebase backend. A network connection is required for
        /// the method to succeed.
        /// </summary>
        Task<string> GetTokenAsync();

        /// <summary>
        /// Registers the app instance with FCM using its Firebase Installation ID. Requires opting into
        /// installation ID registration in the app's Android manifest or iOS Info.plist.
        /// </summary>
        /// <remarks>Subscribe to <see cref="RegistrationReceived"/> before calling this method to receive the registered ID.</remarks>
        Task RegisterAsync();

        /// <summary>
        /// Asynchronously subscribes to a topic. This uses the default FCM registration token to identify the app instance and periodically
        /// sends data to the Firebase backend.
        /// </summary>
        /// <param name="topic">The name of the topic, for example, @“sports”.</param>
        Task SubscribeToTopicAsync(string topic);

        /// <summary>
        /// Asynchronously unsubscribe from a topic. This uses a FCM Token to identify the app instance and periodically sends data to the
        /// Firebase backend.
        /// </summary>
        /// <param name="topic">The name of the topic, for example @“sports”.</param>
        Task UnsubscribeFromTopicAsync(string topic);

        /// <summary>
        /// Call it when a fcm notification was received.
        /// </summary>
        /// <param name="fcmNotification">The received fcm notification.</param>
        void OnNotificationReceived(FCMNotification fcmNotification);

        /// <summary>
        /// Gets invoked when the fcm registration token has changed.
        /// </summary>
        event EventHandler<FCMTokenChangedEventArgs> TokenChanged;

        /// <summary>
        /// Raised when Firebase reports a successful installation ID registration, including registrations from auto-initialization.
        /// </summary>
        event EventHandler<FCMRegistrationReceivedEventArgs> RegistrationReceived;

        /// <summary>
        /// Gets invoked when a new fcm notification was received.
        /// </summary>
        event EventHandler<FCMNotificationReceivedEventArgs> NotificationReceived;

        /// <summary>
        /// Gets invoked when a received fcm notification was tapped by the user.
        /// </summary>
        /// <remarks>
        /// On iOS, <see cref="FCMNotification.Data"/> carries the native response's action identifier under the
        /// <c>ActionIdentifier</c> key, including for a tap kept until the first subscriber is added.
        /// </remarks>
        event EventHandler<FCMNotificationTappedEventArgs> NotificationTapped;

        /// <summary>
        /// Gets invoked when something went wrong during the validity check.
        /// </summary>
        event EventHandler<FCMErrorEventArgs> Error;
    }
}
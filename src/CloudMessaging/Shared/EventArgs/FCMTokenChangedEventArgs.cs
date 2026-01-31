namespace Plugin.Firebase.CloudMessaging.EventArgs
{
    /// <summary>
    /// Event arguments for when the Firebase Cloud Messaging registration token changes.
    /// </summary>
    public sealed class FCMTokenChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Creates a new <c>FCMTokenChangedEventArgs</c> instance.
        /// </summary>
        /// <param name="token">The new FCM registration token.</param>
        public FCMTokenChangedEventArgs(string token)
        {
            Token = token;
        }

        /// <summary>
        /// Gets the new FCM registration token.
        /// </summary>
        public string Token { get; }
    }
}
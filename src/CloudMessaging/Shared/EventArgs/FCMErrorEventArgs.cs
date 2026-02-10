namespace Plugin.Firebase.CloudMessaging.EventArgs
{
    /// <summary>
    /// Event arguments for Firebase Cloud Messaging error events.
    /// </summary>
    public sealed class FCMErrorEventArgs : System.EventArgs
    {
        /// <summary>
        /// Creates a new <c>FCMErrorEventArgs</c> instance.
        /// </summary>
        /// <param name="message">The error message.</param>
        public FCMErrorEventArgs(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message { get; }
    }
}
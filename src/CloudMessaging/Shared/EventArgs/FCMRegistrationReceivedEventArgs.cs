namespace Plugin.Firebase.CloudMessaging.EventArgs
{
    /// <summary>
    /// Event arguments for a successful Firebase Cloud Messaging registration by installation ID.
    /// </summary>
    public sealed class FCMRegistrationReceivedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Creates event arguments with the installation ID reported by Firebase.
        /// </summary>
        /// <param name="installationId">The Firebase Installation ID registered with FCM.</param>
        public FCMRegistrationReceivedEventArgs(string installationId)
        {
            InstallationId = installationId;
        }

        /// <summary>
        /// Gets the Firebase Installation ID registered with FCM.
        /// </summary>
        public string InstallationId { get; }
    }
}
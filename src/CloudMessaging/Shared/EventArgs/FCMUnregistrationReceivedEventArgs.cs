namespace Plugin.Firebase.CloudMessaging.EventArgs
{
    /// <summary>
    /// Event arguments for a Firebase Cloud Messaging unregistration by installation ID.
    /// </summary>
    public sealed class FCMUnregistrationReceivedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Creates event arguments with the installation ID reported by Firebase.
        /// </summary>
        /// <param name="installationId">The Firebase Installation ID unregistered from FCM.</param>
        public FCMUnregistrationReceivedEventArgs(string installationId)
        {
            InstallationId = installationId;
        }

        /// <summary>
        /// Gets the Firebase Installation ID unregistered from FCM.
        /// </summary>
        public string InstallationId { get; }
    }
}

namespace Plugin.Firebase.Firestore
{
    /// <summary>
    /// Settings used to configure a <c>IFirebaseFirestore</c> instance.
    /// </summary>
    public sealed class FirestoreSettings
    {
        public FirestoreSettings(
            string host = null,
            bool isPersistenceEnabled = false,
            bool isSslEnabled = false,
            long cacheSizeBytes = 0)
        {
            Host = host;
            IsPersistenceEnabled = isPersistenceEnabled;
            IsSslEnabled = isSslEnabled;
            CacheSizeBytes = cacheSizeBytes;
        }

        /// <summary>
        /// Returns the host of the Cloud Firestore backend.
        /// </summary>
        public string Host { get; }
        
        /// <summary>
        /// Returns whether or not to use local persistent storage.
        /// </summary>
        public bool IsPersistenceEnabled { get; }
        
        /// <summary>
        /// Returns whether or not to use SSL for communication.
        /// </summary>
        public bool IsSslEnabled { get; }
        
        /// <summary>
        /// Returns the threshold for the cache size above which the SDK will attempt to collect the least recently used documents.
        /// </summary>
        public long CacheSizeBytes { get; }
    }
}
namespace Plugin.Firebase.Firestore
{
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

        public string Host { get; }
        public bool IsPersistenceEnabled { get; }
        public bool IsSslEnabled { get; }
        public long CacheSizeBytes { get; }
    }
}
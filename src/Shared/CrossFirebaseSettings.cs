using Plugin.Firebase.Common;

namespace Plugin.Firebase.Shared
{
    public sealed class CrossFirebaseSettings
    {
        public CrossFirebaseSettings(
            bool isAnalyticsEnabled = false,
            bool isAuthEnabled = false,
            bool isCloudMessagingEnabled = false,
            bool isCrashlyticsEnabled = false,
            bool isDynamicLinksEnabled = false,
            bool isFirestoreEnabled = false,
            bool isFunctionsEnabled = false,
            bool isRemoteConfigEnabled = false,
            bool isStorageEnabled = false,
            string facebookId = null,
            string facebookAppName = null,
            string googleRequestIdToken = null)
        {
            IsAnalyticsEnabled = isAnalyticsEnabled;
            IsAuthEnabled = isAuthEnabled;
            IsCloudMessagingEnabled = isCloudMessagingEnabled;
            IsCrashlyticsEnabled = isCrashlyticsEnabled;
            IsDynamicLinksEnabled = isDynamicLinksEnabled;
            IsFirestoreEnabled = isFirestoreEnabled;
            IsFunctionsEnabled = isFunctionsEnabled;
            IsRemoteConfigEnabled = isRemoteConfigEnabled;
            IsStorageEnabled = isStorageEnabled;
            FacebookId = facebookId;
            FacebookAppName = facebookAppName;
            GoogleRequestIdToken = googleRequestIdToken;
        }

        public override string ToString()
        {
            return $"[{nameof(CrossFirebaseSettings)}: " +
                   $"{nameof(IsAnalyticsEnabled)}={IsAnalyticsEnabled}," +
                   $"{nameof(IsAuthEnabled)}={IsAuthEnabled}," +
                   $"{nameof(IsCloudMessagingEnabled)}={IsCloudMessagingEnabled}," +
                   $"{nameof(IsCrashlyticsEnabled)}={IsCrashlyticsEnabled}," +
                   $"{nameof(IsDynamicLinksEnabled)}={IsDynamicLinksEnabled}," +
                   $"{nameof(IsFirestoreEnabled)}={IsFirestoreEnabled}," +
                   $"{nameof(IsFunctionsEnabled)}={IsFunctionsEnabled}," +
                   $"{nameof(IsRemoteConfigEnabled)}={IsRemoteConfigEnabled}," +
                   $"{nameof(IsStorageEnabled)}={IsStorageEnabled}]";
        }

        public bool IsAnalyticsEnabled { get; }
        public bool IsAuthEnabled { get; }
        public bool IsCloudMessagingEnabled { get; }
        public bool IsCrashlyticsEnabled { get; }
        public bool IsDynamicLinksEnabled { get; }
        public bool IsFirestoreEnabled { get; }
        public bool IsFunctionsEnabled { get; }
        public bool IsRemoteConfigEnabled { get; }
        public bool IsStorageEnabled { get; }

        public string FacebookId { get; }
        public string FacebookAppName { get; }
        public string GoogleRequestIdToken { get; }
    }
}
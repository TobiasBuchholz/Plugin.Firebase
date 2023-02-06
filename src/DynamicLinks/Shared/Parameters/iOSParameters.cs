namespace Plugin.Firebase.DynamicLinks.Parameters
{
    public sealed class iOSParameters
    {
        public iOSParameters(
            string bundleId = null,
            string appStoreId = null,
            string fallbackUrl = null,
            string customScheme = null,
            string iPadFallbackUrl = null,
            string iPadBundleId = null,
            string minimumAppVersion = null)
        {
            BundleId = bundleId;
            AppStoreId = appStoreId;
            FallbackUrl = fallbackUrl;
            CustomScheme = customScheme;
            this.iPadFallbackUrl = iPadFallbackUrl;
            this.iPadBundleId = iPadBundleId;
            MinimumAppVersion = minimumAppVersion;
        }

        public string BundleId { get; }
        public string AppStoreId { get; }
        public string FallbackUrl { get; }
        public string CustomScheme { get; }
        public string iPadFallbackUrl { get; }
        public string iPadBundleId { get; }
        public string MinimumAppVersion { get; }
    }
}
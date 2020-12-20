namespace Plugin.Firebase.Auth
{
    public sealed class ActionCodeSettings
    {
        public void SetAndroidPackageName(string packageName, bool installIfNotAvailable, string minimumVersion)
        {
            AndroidPackageName = packageName;
            AndroidInstallIfNotAvailable = installIfNotAvailable;
            AndroidMinimumVersion = minimumVersion;
        }
        
        public string Url { get; set; }
        public string IOSBundleId { get; set; }
        public string AndroidPackageName { get; private set; }
        public string AndroidMinimumVersion { get; private set; }
        public bool AndroidInstallIfNotAvailable { get; private set; }
        public bool HandleCodeInApp { get; set; }
    }
}
namespace Plugin.Firebase.DynamicLinks.Parameters
{
    public sealed class AndroidParameters
    {
        public AndroidParameters(
            string packageName = null,
            string fallbackUrl = null,
            int minimumVersion = 0)
        {
            PackageName = packageName;
            FallbackUrl = fallbackUrl;
            MinimumVersion = minimumVersion;
        }

        public string PackageName { get; }
        public string FallbackUrl { get; }
        public int MinimumVersion { get; }
    }
}
namespace Playground.Common.Services.Navigation
{
    public static class NavigationPaths
    {
        public const string PageRemoteConfig = "remote_config";
        public const string PageStorage = "storage";

        public static string ToRemoteConfigPage()
        {
            return PageRemoteConfig;
        }

        public static string ToStoragePage()
        {
            return PageStorage;
        }
    }
}
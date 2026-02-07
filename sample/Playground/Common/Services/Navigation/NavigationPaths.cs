namespace Playground.Common.Services.Navigation;

public static class NavigationPaths
{
    public const string PageAuth = "auth";
    public const string PageCloudMessaging = "cloud_messaging";
    public const string PageAppCheck = "app_check";
    public const string PageAppCheckModeSelection = "app_check_mode_selection";
    public const string PageRemoteConfig = "remote_config";
    public const string PageStorage = "storage";

    public static string ToAuthPage()
    {
        return PageAuth;
    }

    public static string ToCloudMessagingPage()
    {
        return PageCloudMessaging;
    }

    public static string ToAppCheckPage()
    {
        return PageAppCheck;
    }

    public static string ToAppCheckModeSelectionPage()
    {
        return PageAppCheckModeSelection;
    }

    public static string ToRemoteConfigPage()
    {
        return PageRemoteConfig;
    }

    public static string ToStoragePage()
    {
        return PageStorage;
    }
}
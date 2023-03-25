using Playground.Features.Auth;
using Playground.Features.CloudMessaging;
using Playground.Features.RemoteConfig;
using Playground.Features.Storage;

namespace Playground;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(NavigationPaths.PageAuth, typeof(AuthPage));
        Routing.RegisterRoute(NavigationPaths.PageCloudMessaging, typeof(CloudMessagingPage));
        Routing.RegisterRoute(NavigationPaths.PageRemoteConfig, typeof(RemoteConfigPage));
        Routing.RegisterRoute(NavigationPaths.PageStorage, typeof(StoragePage));
    }
}
using System;
using Playground.Common.Services.Navigation;
using Playground.Features.RemoteConfig;
using Playground.Features.Storage;
using Xamarin.Forms;

namespace Playground
{
    public partial class AppShell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }
        
        private static void RegisterRoutes()
        {
            Routing.RegisterRoute(NavigationPaths.PageRemoteConfig, typeof(RemoteConfigPage));
            Routing.RegisterRoute(NavigationPaths.PageStorage, typeof(StoragePage));
        }
    }
}

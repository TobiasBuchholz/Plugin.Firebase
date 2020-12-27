using Playground.Common.Base;
using Playground.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Vm = Playground.Features.Dashboard.DashboardViewModel;

namespace Playground.Features.Dashboard
{
    public sealed class DashboardPage : ContentPageBase<Vm>
    {
        public DashboardPage() => Build();

        private void Build()
        {
            InitializeViewModel();
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;
            BackgroundColor = Color.White;

            Content = new StackLayout {
                    Visual = VisualMarker.Material,
                    Children = {
                        new Button { Text = Localization.ButtonAuth, TextColor = Color.Black }
                            .Bind(nameof(Vm.NavigateToAuthPageCommand)),
                        new Button { Text = Localization.ButtonCloudMessaging, TextColor = Color.Black }
                            .Bind(nameof(Vm.NavigateToCloudMessagingPageCommand)),
                        new Button { Text = Localization.ButtonRemoteConfig, TextColor = Color.Black }
                            .Bind(nameof(Vm.NavigateToRemoteConfigPageCommand)),
                        new Button { Text = Localization.ButtonStorage, TextColor = Color.Black }
                            .Bind(nameof(Vm.NavigateToStoragePageCommand))
                    }
                }
                .FillHorizontal()
                .CenterVertical()
                .Margin(24);
        }
    }
}
using Vm = Playground.Features.Dashboard.DashboardViewModel;

namespace Playground.Features.Dashboard;

[Preserve(AllMembers = true)]
public sealed class DashboardPage : ContentPageBase
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        BindingContext = viewModel;
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;
        Build();
    }

    private void Build()
    {
        Content = new VerticalStackLayout {
            Spacing = 4,
            Children = {
                    new Button { Text = Localization.ButtonAuth }
                        .Bind(nameof(Vm.NavigateToAuthPageCommand)),
                    new Button { Text = Localization.ButtonCloudMessaging }
                        .Bind(nameof(Vm.NavigateToCloudMessagingPageCommand)),
                    new Button { Text = Localization.ButtonRemoteConfig }
                        .Bind(nameof(Vm.NavigateToRemoteConfigPageCommand)),
                    new Button { Text = Localization.ButtonStorage }
                        .Bind(nameof(Vm.NavigateToStoragePageCommand))
                }
        }
            .FillHorizontal()
            .CenterVertical()
            .Margin(24);
    }
}
using Vm = Playground.Features.Dashboard.DashboardViewModel;

namespace Playground.Features.Dashboard;

[Preserve(AllMembers = true)]
public sealed class DashboardPage : ContentPageBase
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        BindingContext = viewModel;
        BackgroundColor = Colors.White;
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;
        
        Build();   
    }

    private void Build()
    {
        Content = new StackLayout {
            Children = {
                    new Button { Text = Localization.ButtonAuth, TextColor = Colors.Black }
                        .Bind(nameof(Vm.NavigateToAuthPageCommand)),
                    new Button { Text = Localization.ButtonCloudMessaging, TextColor = Colors.Black }
                        .Bind(nameof(Vm.NavigateToCloudMessagingPageCommand)),
                    new Button { Text = Localization.ButtonRemoteConfig, TextColor = Colors.Black }
                        .Bind(nameof(Vm.NavigateToRemoteConfigPageCommand)),
                    new Button { Text = Localization.ButtonStorage, TextColor = Colors.Black }
                        .Bind(nameof(Vm.NavigateToStoragePageCommand))
                }
            }
            .FillHorizontal()
            .CenterVertical()
            .Margin(24);
    }
}
using Vm = Playground.Features.AppCheck.AppCheckViewModel;

namespace Playground.Features.AppCheck;

[Preserve(AllMembers = true)]
public sealed class AppCheckPage : ContentPageBase
{
    public AppCheckPage(AppCheckViewModel viewModel)
    {
        BindingContext = viewModel;
        Build();
    }

    private void Build()
    {
        Content = new VerticalStackLayout {
            Spacing = 8,
            Children = {
                    new Label {
                        Text = "Configure the native AppCheck provider for this Playground session."
                    },
                    new Label {
                        FontAttributes = FontAttributes.Bold
                    }
                    .Bind(nameof(Vm.StatusMessage)),
                    new Button {
                        Text = "AppCheck: Disabled"
                    }
                    .Bind(nameof(Vm.ConfigureDisabledCommand)),
                    new Button {
                        Text = "AppCheck: Debug"
                    }
                    .Bind(nameof(Vm.ConfigureDebugCommand)),
                    new Button {
                        Text = "AppCheck: DeviceCheck (iOS)"
                    }
                    .Bind(nameof(Vm.ConfigureDeviceCheckCommand)),
                    new Button {
                        Text = "AppCheck: AppAttest (iOS)"
                    }
                    .Bind(nameof(Vm.ConfigureAppAttestCommand)),
                    new Button {
                        Text = "AppCheck: PlayIntegrity (Android)"
                    }
                    .Bind(nameof(Vm.ConfigurePlayIntegrityCommand))
                }
        }
        .Margin(24);
    }
}
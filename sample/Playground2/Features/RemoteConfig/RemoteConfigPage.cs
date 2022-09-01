using Vm = Playground.Features.RemoteConfig.RemoteConfigViewModel;

namespace Playground.Features.RemoteConfig;

[Preserve(AllMembers = true)]
public sealed class RemoteConfigPage : ContentPageBase
{
    public RemoteConfigPage(RemoteConfigViewModel viewModel)
    {
        BindingContext = viewModel;
        BackgroundColor = Colors.White;
        
        Build();
    }

    private void Build()
    {
        Content = new Grid {
            Children = {
                new Label { TextColor = Colors.Black }
                    .Center()
                    .Bind(nameof(Vm.SomeRemoteConfigValue)),
                new Button { Text = Localization.ButtonFetchRemoteConfig, TextColor = Colors.Black }
                    .Bottom()
                    .Bind(nameof(Vm.FetchAndActivateCommand)),
            }
        }
            .Margin(32);
    }
}
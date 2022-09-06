using Vm = Playground.Features.RemoteConfig.RemoteConfigViewModel;

namespace Playground.Features.RemoteConfig;

[Preserve(AllMembers = true)]
public sealed class RemoteConfigPage : ContentPageBase
{
    public RemoteConfigPage(RemoteConfigViewModel viewModel)
    {
        BindingContext = viewModel;
        Build();
    }

    private void Build()
    {
        Content = new Grid {
                Children = {
                    new Label()
                        .Center()
                        .Bind(nameof(Vm.SomeRemoteConfigValue)),
                    new Button { Text = Localization.ButtonFetchRemoteConfig }
                        .Bottom()
                        .Bind(nameof(Vm.FetchAndActivateCommand)),
                }
            }
            .Margin(32);
    }
}
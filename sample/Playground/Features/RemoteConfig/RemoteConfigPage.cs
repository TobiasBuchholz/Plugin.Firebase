using Playground.Common.Base;
using Playground.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Color = System.Drawing.Color;
using Vm = Playground.Features.RemoteConfig.RemoteConfigViewModel;

namespace Playground.Features.RemoteConfig
{
    public sealed class RemoteConfigPage : ContentPageBase<Vm>
    {
        public RemoteConfigPage() => Build();

        private void Build()
        {
            InitializeViewModel();
            BackgroundColor = Color.White;

            Content = new Grid {
                    Children = {
                    new Label { TextColor = Color.Black }
                        .Center()
                        .Bind(nameof(Vm.SomeRemoteConfigValue)),
                    new Button { Text = Localization.ButtonFetchRemoteConfig, TextColor = Color.Black }
                        .Bottom()
                        .Bind(nameof(Vm.FetchAndActivateCommand)),
                }
            }
                .Margin(32);
        }
    }
}
using Playground.Common.Base;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Color = System.Drawing.Color;
using Vm = Playground.Features.Storage.StorageViewModel;

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
                    new Label {
                            Text = "Hello Remote Config",
                            TextColor = Color.Black
                        }
                        .Center()
                }
            };
        }
    }
}
using Playground.Common.Base;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Color = System.Drawing.Color;
using Vm = Playground.Features.Storage.StorageViewModel;

namespace Playground.Features.Storage
{
    public sealed class StoragePage : ContentPageBase<Vm>
    {
        public StoragePage() => Build();

        private void Build()
        {
            InitializeViewModel();
            BackgroundColor = Color.White;

            Content = new Grid {
                Children = {
                    new Label {
                            Text = "Hello Storage",
                            TextColor = Color.Black
                        }
                        .Center()
                }
            };
        }
    }
}
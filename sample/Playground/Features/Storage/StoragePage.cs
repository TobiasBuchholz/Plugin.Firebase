using Playground.Common.Base;
using Playground.Resources;
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
                    new Entry {
                            Placeholder = Localization.EntryPlaceholderStorage,
                            TextColor = Color.Black
                        }
                        .CenterVertical()
                        .FillHorizontal()
                        .Bind(nameof(Vm.Text), BindingMode.TwoWay),
                    new Button { Text = Localization.ButtonTextUploadText, TextColor = Color.Black }
                        .Bottom()
                        .Bind(nameof(Vm.UploadTextCommand)),
                }
            }
                .Margin(32);
        }
    }
}
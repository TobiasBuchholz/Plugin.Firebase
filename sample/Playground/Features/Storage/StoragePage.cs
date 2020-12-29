using Playground.Common.Base;
using Playground.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.Internals;
using Color = System.Drawing.Color;
using Vm = Playground.Features.Storage.StorageViewModel;

namespace Playground.Features.Storage
{
    [Preserve(AllMembers = true)]
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
                            PlaceholderColor = Color.DarkGray,
                            TextColor = Color.Black
                        }
                        .CenterVertical()
                        .FillHorizontal()
                        .Bind(nameof(Vm.Text), BindingMode.TwoWay),
                    new Button { Text = Localization.ButtonUploadText, TextColor = Color.Black }
                        .Bottom()
                        .Bind(nameof(Vm.UploadTextCommand)),
                }
            }
                .Margin(32);
        }
    }
}
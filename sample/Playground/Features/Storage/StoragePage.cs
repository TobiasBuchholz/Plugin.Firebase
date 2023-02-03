using Vm = Playground.Features.Storage.StorageViewModel;

namespace Playground.Features.Storage;

[Preserve(AllMembers = true)]
public sealed class StoragePage : ContentPageBase
{
    public StoragePage(StorageViewModel viewModel)
    {
        BindingContext = viewModel;
        Build();
    }

    private void Build()
    {
        Content = new Grid {
                Children = {
                    new Entry { Placeholder = Localization.EntryPlaceholderStorage }
                        .CenterVertical()
                        .FillHorizontal()
                        .Bind(nameof(Vm.Text), BindingMode.TwoWay),
                    new Button { Text = Localization.ButtonUploadText }
                        .Bottom()
                        .Bind(nameof(Vm.UploadTextCommand)),
                }
            }
            .Margin(32);
    }
}
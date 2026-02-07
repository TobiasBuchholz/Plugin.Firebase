using Vm = Playground.Features.AppCheck.AppCheckModeSelectionViewModel;

namespace Playground.Features.AppCheck;

[Preserve(AllMembers = true)]
public sealed class AppCheckModeSelectionPage : ContentPageBase
{
    public AppCheckModeSelectionPage(AppCheckModeSelectionViewModel viewModel)
    {
        BindingContext = viewModel;
        Title = "Select App Check Mode";
        Build();
    }

    private void Build()
    {
        var headerLabel = new Label {
            FontAttributes = FontAttributes.Bold,
            LineBreakMode = LineBreakMode.WordWrap,
            FontSize = 16
        }
        .Bind(nameof(Vm.HeaderText));

        var statusLabel = new Label {
            FontSize = 14,
            LineBreakMode = LineBreakMode.WordWrap,
            Margin = new Thickness(0, 16, 0, 0)
        }
        .Bind(nameof(Vm.StatusMessage));

        var modes = new[] { "Disabled", "Debug", "Device Check", "App Attest" };
        var stackView = new VerticalStackLayout { Spacing = 12 };

        foreach(var mode in modes) {
            var button = new Button {
                Text = mode,
                FontSize = 14,
                Padding = new Thickness(12, 16),
                CornerRadius = 8,
                CommandParameter = mode
            };

            button.SetBinding(Button.CommandProperty, nameof(Vm.SelectModeCommand));
            stackView.Add(button);
        }

        Content = new VerticalStackLayout {
            Spacing = 24,
            Padding = new Thickness(24),
            Children =
            {
                headerLabel,
                statusLabel,
                new ScrollView
                {
                    Content = stackView.FillHorizontal()
                }
            }
        }
        .FillHorizontal()
        .CenterVertical();
    }
}
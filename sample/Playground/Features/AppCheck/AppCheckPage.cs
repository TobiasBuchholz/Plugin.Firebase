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
        var tokenLabel = new Label {
            FontFamily = "CourierNewPSMT",
            LineBreakMode = LineBreakMode.CharacterWrap
        }
        .Bind(nameof(Vm.CurrentToken));

        var copyTokenTapGestureRecognizer = new TapGestureRecognizer();
        copyTokenTapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, nameof(Vm.CopyTokenCommand));
        tokenLabel.GestureRecognizers.Add(copyTokenTapGestureRecognizer);

        var resetButton = new Button {
            Text = "Change Mode",
            TextColor = Colors.Orange,
            Margin = new Thickness(0, 24, 0, 0)
        }
        .Bind(nameof(Vm.ResetModeCommand));

        var modeNoteLabel = new Label {
            Text = "ℹ️ To test a different App Check mode, tap 'Change Mode' to select another provider. You must then close and restart the app manually.",
            FontSize = 12,
            LineBreakMode = LineBreakMode.WordWrap,
            Margin = new Thickness(0, 32, 0, 0),
            TextColor = Colors.Gray
        };

        var contentStack = new VerticalStackLayout {
            Spacing = 8,
            Children = {
                    new Label {
                        Text = "Current Mode:",
                        FontSize = 14
                    },
                    new Label {
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 16
                    }
                    .Bind(nameof(Vm.ConfiguredProvider)),
                    new Label {
                        FontAttributes = FontAttributes.Bold,
                        Margin = new Thickness(0, 16, 0, 0)
                    }
                    .Bind(nameof(Vm.StatusMessage)),
                    new Label {
                        Text = "Tap the token text to copy it."
                    },
                    tokenLabel,
                    new Button {
                        Text = "Fetch AppCheck Token"
                    }
                    .Bind(nameof(Vm.FetchTokenCommand)),
                    resetButton,
                    modeNoteLabel
                }
        };

        Content = new ScrollView {
            Content = contentStack.Margin(24)
        };
    }
}
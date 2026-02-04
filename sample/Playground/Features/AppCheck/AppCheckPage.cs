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

        Content = new VerticalStackLayout {
            Spacing = 8,
            Children = {
                    new Label {
                        Text = "AppCheck provider configured in MauiProgram (CrossFirebaseSettings.AppCheckOptions):"
                    },
                    new Label {
                        FontAttributes = FontAttributes.Bold
                    }
                    .Bind(nameof(Vm.ConfiguredProvider)),
                    new Label {
                        FontAttributes = FontAttributes.Bold
                    }
                    .Bind(nameof(Vm.StatusMessage)),
                    new Label {
                        Text = "Tap the token text to copy it."
                    },
                    tokenLabel,
                    new Button {
                        Text = "Fetch AppCheck Token"
                    }
                    .Bind(nameof(Vm.FetchTokenCommand))
                }
        }
        .Margin(24);
    }
}
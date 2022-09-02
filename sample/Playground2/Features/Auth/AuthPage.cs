using Vm = Playground.Features.Auth.AuthViewModel;

namespace Playground.Features.Auth;

[Preserve(AllMembers = true)]
public sealed class AuthPage : ContentPageBase
{
    public AuthPage(AuthViewModel viewModel)
    {
        BindingContext = viewModel;
        Build();
    }

    private void Build()
    {
        Content = new Grid {
                RowDefinitions = Rows.Define(Star, Star),
                RowSpacing = 4,
                Children = {
                    new Label { HorizontalTextAlignment = TextAlignment.Center }
                        .Row(0)
                        .Center()
                        .Bind(nameof(Vm.LoginText)),
                    CreateSignInButtonsLayout()
                        .Row(1)
                        .FillHorizontal()
                        .Top()
                        .Bind(IsVisibleProperty, nameof(Vm.ShowsSignInButtons)),
                    CreateLinkingButtonsLayout()
                        .Row(1)
                        .FillHorizontal()
                        .Top()
                        .Bind(IsVisibleProperty, nameof(Vm.ShowsLinkingButtons)),
                    new ActivityIndicator()
                        .Row(1)
                        .CenterHorizontal()
                        .Top()
                        .Bind(ActivityIndicator.IsRunningProperty, nameof(Vm.IsInProgress)),
                    CreateSignOutButtonsLayout()
                        .Row(1)
                        .FillHorizontal()
                        .Bottom()
                        .Bind(IsVisibleProperty, nameof(Vm.ShowsSignOutButtons))
                }
            }
            .Margin(24);
    }

    private static ScrollView CreateSignInButtonsLayout()
    {
        return new ScrollView {
            Content = new VerticalStackLayout {
                Spacing = 4,
                Children = {
                  new Button { Text = Localization.ButtonSignInAnonymously }
                      .FillHorizontal()
                      .Bind(nameof(Vm.SignInAnonymouslyCommand)),
                  new Button { Text = Localization.ButtonSignInWithEmail }
                      .FillHorizontal()
                      .Bind(nameof(Vm.SignInWithEmailCommand)),
                  new Button { Text = Localization.ButtonSignInWithEmailLink }
                      .FillHorizontal()
                      .Bind(nameof(Vm.SignInWithEmailLinkCommand)),
                  new Button { Text = Localization.ButtonSignInWithGoogle }
                      .FillHorizontal()
                      .Bind(nameof(Vm.SignInWithGoogleCommand)),
                  new Button { Text = Localization.ButtonSignInWithFacebook }
                      .FillHorizontal()
                      .Bind(nameof(Vm.SignInWithFacebookCommand)),
                  new Button { Text = Localization.ButtonSignInWithApple }
                      .FillHorizontal()
                      .Bind(nameof(Vm.SignInWithAppleCommand)),
                  new Button { Text = Localization.ButtonSignInWithPhoneNumber }
                      .FillHorizontal()
                      .Bind(nameof(Vm.SignInWithPhoneNumberCommand))
                }
            }
        };
    }

    private static VerticalStackLayout CreateLinkingButtonsLayout()
    {
        return new VerticalStackLayout {
            Spacing = 4,
            Children = {
                new Button { Text = Localization.ButtonLinkWithEmail }
                    .FillHorizontal()
                    .Bind(nameof(Vm.LinkWithEmailCommand)),
                new Button { Text = Localization.ButtonLinkWithGoogle }
                    .FillHorizontal()
                    .Bind(nameof(Vm.LinkWithGoogleCommand)),
                new Button { Text = Localization.ButtonLinkWithFacebook }
                    .FillHorizontal()
                    .Bind(nameof(Vm.LinkWithFacebookCommand)),
                new Button { Text = Localization.ButtonLinkWithPhoneNumber }
                    .FillHorizontal()
                    .Bind(nameof(Vm.LinkWithPhoneNumberCommand))
            }
        };
    }

    private static VerticalStackLayout CreateSignOutButtonsLayout()
    {
        return new VerticalStackLayout {
            Spacing = 4,
            Children = {
                new Button { Text = Localization.ButtonUnlinkProvider }
                    .FillHorizontal()
                    .Bind(nameof(Vm.UnlinkProviderCommand)),
                new Button { Text = Localization.ButtonSendEmailToResetPassword }
                    .FillHorizontal()
                    .Bind(nameof(Vm.SendPasswordResetEmailCommand)),
                new Button { Text = Localization.ButtonSignOut }
                    .FillHorizontal()
                    .Bind(nameof(Vm.SignOutCommand))
            }
        };
    }
}
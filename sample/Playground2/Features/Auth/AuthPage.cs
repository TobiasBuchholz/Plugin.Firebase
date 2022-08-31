using Microsoft.Maui.Controls.Internals;
using Playground.Resources;
using Vm = Playground.Features.Auth.AuthViewModel;
using CommunityToolkit.Maui.Markup;
using Playground.Common.Base;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace Playground.Features.Auth
{
    [Preserve(AllMembers = true)]
    public sealed class AuthPage : ContentPageBase
    {
        public AuthPage(AuthViewModel viewModel)
        {
            BindingContext = viewModel;
            BackgroundColor = Colors.White;
            
            Build();
        }

        private void Build()
        {
            Content = new Grid {
                RowDefinitions = Rows.Define(Star, Star),
                Children = {
                        new Label { TextColor = Colors.Black, HorizontalTextAlignment = TextAlignment.Center }
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
                        new ActivityIndicator { Color = Colors.Black }
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
                Content = new StackLayout {
                    Orientation = StackOrientation.Vertical,
                    Children = {
                      new Button { Text = Localization.ButtonSignInAnonymously, TextColor = Colors.Black }
                          .FillHorizontal()
                          .Bind(nameof(Vm.SignInAnonymouslyCommand)),
                      new Button { Text = Localization.ButtonSignInWithEmail, TextColor = Colors.Black }
                          .FillHorizontal()
                          .Bind(nameof(Vm.SignInWithEmailCommand)),
                      new Button { Text = Localization.ButtonSignInWithEmailLink, TextColor = Colors.Black }
                          .FillHorizontal()
                          .Bind(nameof(Vm.SignInWithEmailLinkCommand)),
                      new Button { Text = Localization.ButtonSignInWithGoogle, TextColor = Colors.Black }
                          .FillHorizontal()
                          .Bind(nameof(Vm.SignInWithGoogleCommand)),
                      new Button { Text = Localization.ButtonSignInWithFacebook, TextColor = Colors.Black }
                          .FillHorizontal()
                          .Bind(nameof(Vm.SignInWithFacebookCommand)),
                      new Button { Text = Localization.ButtonSignInWithApple, TextColor = Colors.Black }
                          .FillHorizontal()
                          .Bind(nameof(Vm.SignInWithAppleCommand)),
                      new Button { Text = Localization.ButtonSignInWithPhoneNumber, TextColor = Colors.Black }
                          .FillHorizontal()
                          .Bind(nameof(Vm.SignInWithPhoneNumberCommand))
                    }
                }
            };
        }

        private static StackLayout CreateLinkingButtonsLayout()
        {
            return new StackLayout {
                Orientation = StackOrientation.Vertical,
                Children = {
                    new Button { Text = Localization.ButtonLinkWithEmail, TextColor = Colors.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.LinkWithEmailCommand)),
                    new Button { Text = Localization.ButtonLinkWithGoogle, TextColor = Colors.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.LinkWithGoogleCommand)),
                    new Button { Text = Localization.ButtonLinkWithFacebook, TextColor = Colors.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.LinkWithFacebookCommand)),
                    new Button { Text = Localization.ButtonLinkWithPhoneNumber, TextColor = Colors.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.LinkWithPhoneNumberCommand))
                }
            };
        }

        private static StackLayout CreateSignOutButtonsLayout()
        {
            return new StackLayout {
                Orientation = StackOrientation.Vertical,
                Children = {
                    new Button { Text = Localization.ButtonUnlinkProvider, TextColor = Colors.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.UnlinkProviderCommand)),
                    new Button { Text = Localization.ButtonSendEmailToResetPassword, TextColor = Colors.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.SendPasswordResetEmailCommand)),
                    new Button { Text = Localization.ButtonSignOut, TextColor = Colors.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.SignOutCommand))
                }
            };
        }
    }
}
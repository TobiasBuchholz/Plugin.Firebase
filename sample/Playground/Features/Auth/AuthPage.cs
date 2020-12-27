using Playground.Common.Base;
using Playground.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Vm = Playground.Features.Auth.AuthViewModel;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace Playground.Features.Auth
{
    public sealed class AuthPage : ContentPageBase<Vm>
    {
        public AuthPage() => Build();

        private void Build()
        {
            InitializeViewModel();
            
            Content = new Grid {
                    RowDefinitions = Rows.Define(Star, Star),
                    Children = {
                        new Label { TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center }
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
                        new ActivityIndicator { Color = Color.Black }
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

        private static StackLayout CreateSignInButtonsLayout()
        {
            return new StackLayout {
                Orientation = StackOrientation.Vertical,
                Children = {
                    new Button { Text = Localization.ButtonSignInAnonymously, TextColor = Color.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.SignInAnonymouslyCommand)),
                    new Button { Text = Localization.ButtonSignInWithEmail, TextColor = Color.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.SignInWithEmailCommand)),
                    new Button { Text = Localization.ButtonSignInWithEmailLink, TextColor = Color.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.SignInWithEmailLinkCommand)),
                    new Button { Text = Localization.ButtonSignInWithGoogle, TextColor = Color.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.SignInWithGoogleCommand)),
                    new Button { Text = Localization.ButtonSignInWithFacebook, TextColor = Color.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.SignInWithFacebookCommand)),
                    new Button { Text = Localization.ButtonSignInWithPhoneNumber, TextColor = Color.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.SignInWithPhoneNumberCommand)),
                }
            };
        }

        private static StackLayout CreateLinkingButtonsLayout()
        {
            return new StackLayout {
                Orientation = StackOrientation.Vertical,
                Children = {
                    new Button { Text = Localization.ButtonLinkWithEmail, TextColor = Color.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.LinkWithEmailCommand)),
                    new Button { Text = Localization.ButtonLinkWithGoogle, TextColor = Color.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.LinkWithGoogleCommand)),
                    new Button { Text = Localization.ButtonLinkWithFacebook, TextColor = Color.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.LinkWithFacebookCommand)),
                    new Button { Text = Localization.ButtonLinkWithPhoneNumber, TextColor = Color.Black }
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
                    new Button { Text = Localization.ButtonUnlinkProvider, TextColor = Color.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.UnlinkProviderCommand)),
                    new Button { Text = Localization.ButtonSignOut, TextColor = Color.Black }
                        .FillHorizontal()
                        .Bind(nameof(Vm.SignOutCommand))
                }
            };
        }
    }
}
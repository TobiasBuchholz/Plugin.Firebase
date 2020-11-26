using Playground.Common.Base;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Vm = Playground.Features.Auth.AuthViewModel;

namespace Playground.Features.Auth
{
    public sealed class AuthPage : ContentPageBase<Vm>
    {
        public AuthPage() => Build();

        private void Build()
        {
            InitializeViewModel();
            
            Content = new Grid {
                Children = {
                    new Label { Text = "Hello Auth" }
                        .Center()
                }
            };
        }
    }
}
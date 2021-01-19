using Playground.Common.Base;
using Playground.Resources;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Vm = Playground.Features.CloudMessaging.CloudMessagingViewModel;

namespace Playground.Features.CloudMessaging
{
    [Preserve(AllMembers = true)]
    public sealed class CloudMessagingPage : ContentPageBase<Vm>
    {
        public CloudMessagingPage() => Build();

        private void Build()
        {
            InitializeViewModel();

            Content = new Grid {
                    Children = {
                        new Button { Text = Localization.ButtonCheckFcmValidity, TextColor = Color.Black }
                            .Row(0)
                            .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                            .Bind(nameof(Vm.CheckIfValidCommand)),
                        new Button { Text = Localization.ButtonSubscribeToTopic, TextColor = Color.Black }
                            .Row(1)
                            .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                            .Bind(nameof(Vm.SubscribeToTopicCommand)),
                        new Button { Text = Localization.ButtonUnsubscribeFromTopic, TextColor = Color.Black }
                            .Row(2)
                            .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                            .Bind(nameof(Vm.UnsubscribeFromTopicCommand)),
                        new Button { Text = Localization.ButtonTriggerNotificationViaToken, TextColor = Color.Black }
                            .Row(3)
                            .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                            .Bind(nameof(Vm.TriggerNotificationViaTokenCommand)),
                        new Button { Text = Localization.ButtonTriggerNotificationViaTopic, TextColor = Color.Black }
                            .Row(4)
                            .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                            .Bind(nameof(Vm.TriggerNotificationViaTopicCommand)),
                        new ActivityIndicator { Color = Color.Black }
                            .Row(0)
                            .RowSpan(5)
                            .Center()
                            .Bind(ActivityIndicator.IsRunningProperty, nameof(Vm.IsInProgress)),
                    }
                }
                .FillHorizontal()
                .CenterVertical()
                .Margin(24);
        }
    }
}
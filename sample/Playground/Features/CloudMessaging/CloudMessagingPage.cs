using Vm = Playground.Features.CloudMessaging.CloudMessagingViewModel;

namespace Playground.Features.CloudMessaging;

[Preserve(AllMembers = true)]
public sealed class CloudMessagingPage : ContentPageBase
{
    public CloudMessagingPage(CloudMessagingViewModel viewModel)
    {
        BindingContext = viewModel;
        Build();
    }

    private void Build()
    {
        Content = new VerticalStackLayout() {
            Spacing = 4,
            Children = {
                    new Button { Text = Localization.ButtonCheckFcmValidity }
                        .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                        .Bind(nameof(Vm.CheckIfValidCommand)),
                    new Button { Text = Localization.ButtonSubscribeToTopic }
                        .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                        .Bind(nameof(Vm.SubscribeToTopicCommand)),
                    new Button { Text = Localization.ButtonUnsubscribeFromTopic }
                        .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                        .Bind(nameof(Vm.UnsubscribeFromTopicCommand)),
                    new Button { Text = Localization.ButtonTriggerNotificationViaToken }
                        .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                        .Bind(nameof(Vm.TriggerNotificationViaTokenCommand)),
                    new Button { Text = Localization.ButtonTriggerNotificationViaTopic }
                        .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                        .Bind(nameof(Vm.TriggerNotificationViaTopicCommand)),
                    new ActivityIndicator()
                        .Center()
                        .Bind(ActivityIndicator.IsRunningProperty, nameof(Vm.IsInProgress)),
                }
        }
            .FillHorizontal()
            .CenterVertical()
            .Margin(24);
    }
}
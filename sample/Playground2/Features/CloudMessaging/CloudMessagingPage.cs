using Vm = Playground.Features.CloudMessaging.CloudMessagingViewModel;

namespace Playground.Features.CloudMessaging;

[Preserve(AllMembers = true)]
public sealed class CloudMessagingPage : ContentPageBase
{
    public CloudMessagingPage(CloudMessagingViewModel viewModel)
    {
        BindingContext = viewModel;
        BackgroundColor = Colors.White;
        
        Build();
    }

    private void Build()
    {
        Content = new VerticalStackLayout() {
            Children = {
                    new Button { Text = Localization.ButtonCheckFcmValidity, TextColor = Colors.Black }
                        .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                        .Bind(nameof(Vm.CheckIfValidCommand)),
                    new Button { Text = Localization.ButtonSubscribeToTopic, TextColor = Colors.Black }
                        .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                        .Bind(nameof(Vm.SubscribeToTopicCommand)),
                    new Button { Text = Localization.ButtonUnsubscribeFromTopic, TextColor = Colors.Black }
                        .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                        .Bind(nameof(Vm.UnsubscribeFromTopicCommand)),
                    new Button { Text = Localization.ButtonTriggerNotificationViaToken, TextColor = Colors.Black }
                        .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                        .Bind(nameof(Vm.TriggerNotificationViaTokenCommand)),
                    new Button { Text = Localization.ButtonTriggerNotificationViaTopic, TextColor = Colors.Black }
                        .Bind(IsVisibleProperty, nameof(Vm.IsInProgress), convert:Negate)
                        .Bind(nameof(Vm.TriggerNotificationViaTopicCommand)),
                    new ActivityIndicator { Color = Colors.Black }
                        .Center()
                        .Bind(ActivityIndicator.IsRunningProperty, nameof(Vm.IsInProgress)),
                }
            }
            .FillHorizontal()
            .CenterVertical()
            .Margin(24);
    }
}
using Playground.Common.Services.PushNotification;

namespace Playground.Features.CloudMessaging;

[Preserve(AllMembers = true)]
public sealed class CloudMessagingViewModel : ViewModelBase
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IUserInteractionService _userInteractionService;
    
    public CloudMessagingViewModel(
        IPushNotificationService pushNotificationService,
        IUserInteractionService userInteractionService)
    {
        _pushNotificationService = pushNotificationService;
        _userInteractionService = userInteractionService;
    
        InitCommands();
        InitProperties();
    }

    private void InitCommands()
    {
        CheckIfValidCommand = ReactiveCommand.CreateFromTask(CheckIfValidAsync);
        SubscribeToTopicCommand = ReactiveCommand.CreateFromTask(SubscribeToTopicAsync);
        UnsubscribeFromTopicCommand = ReactiveCommand.CreateFromTask(UnsubscribeFromTopicAsync);
        TriggerNotificationViaTokenCommand = ReactiveCommand.CreateFromTask(TriggerNotificationViaTokenAsync);
        TriggerNotificationViaTopicCommand = ReactiveCommand.CreateFromTask(TriggerNotificationViaTopicAsync);
    
        Observable
            .Merge(
                CheckIfValidCommand.ThrownExceptions,
                SubscribeToTopicCommand.ThrownExceptions,
                UnsubscribeFromTopicCommand.ThrownExceptions,
                TriggerNotificationViaTokenCommand.ThrownExceptions,
                TriggerNotificationViaTopicCommand.ThrownExceptions)
            .LogThrownException()
            .Subscribe(e => _userInteractionService.ShowErrorDialogAsync(Localization.DialogTitleUnexpectedError, e))
            .DisposeWith(Disposables);
    }
    
    private async Task CheckIfValidAsync()
    {
        await _pushNotificationService.CheckIfValidAsync();
        await _userInteractionService.ShowDefaultDialogAsync(Localization.DialogTitleCloudMessaging, Localization.DialogMessageFcmValid);
    }
    
    private async Task SubscribeToTopicAsync()
    {
        var topic = await AskForTopicAsync();
        if(!string.IsNullOrEmpty(topic)) {
            await _pushNotificationService.SubscribeToTopicAsync(topic);
        }
    }
    
    private Task<string> AskForTopicAsync()
    {
        return _userInteractionService.ShowAsPromptAsync(new UserInfoBuilder()
            .WithTitle(Localization.DialogTitleCloudMessaging)
            .WithMessage(Localization.DialogMessageEnterTopic)
            .WithDefaultButton(Localization.Continue)
            .WithCancelButton(Localization.Cancel)
            .Build());
    }
    
    private async Task UnsubscribeFromTopicAsync()
    {
        var topic = await AskForTopicAsync();
        if(!string.IsNullOrEmpty(topic)) {
            await _pushNotificationService.UnsubscribeFromTopicAsync(topic);
        }
    }
    
    private async Task TriggerNotificationViaTokenAsync()
    {
        var messageBody = await AskForMessageBoyAsync();
        if(!string.IsNullOrEmpty(messageBody)) {
            await TriggerNotificationViaTokenAsync(messageBody);
        }
    }
    
    private async Task TriggerNotificationViaTokenAsync(string messageBody)
    {
        var token = await _pushNotificationService.GetFcmTokenAsync();
        await _pushNotificationService.TriggerNotificationViaTokensAsync(new[] { token }, "Notification via token", messageBody);
    }
    
    private Task<string> AskForMessageBoyAsync()
    {
        return _userInteractionService.ShowAsPromptAsync(new UserInfoBuilder()
            .WithTitle(Localization.DialogTitleCloudMessaging)
            .WithMessage(Localization.DialogMessageEnterCloudMessageBody)
            .WithDefaultButton(Localization.ButtonTriggerNotification)
            .WithCancelButton(Localization.Cancel)
            .Build());
    }
    
    private async Task TriggerNotificationViaTopicAsync()
    {
        var topic = await AskForTopicAsync();
        if(!string.IsNullOrEmpty(topic)) {
            await TriggerNotificationViaTopicAsync(topic);
        }
    }
    
    private async Task TriggerNotificationViaTopicAsync(string topic)
    {
        var messageBody = await AskForMessageBoyAsync();
        if(!string.IsNullOrEmpty(messageBody)) {
            await _pushNotificationService.TriggerNotificationViaTopicAsync(topic, $"Notification via topic: {topic}", messageBody);
        }
    }
    
    private void InitProperties()
    {
        Observable
            .Merge(
                CheckIfValidCommand.IsExecuting,
                SubscribeToTopicCommand.IsExecuting,
                UnsubscribeFromTopicCommand.IsExecuting,
                TriggerNotificationViaTokenCommand.IsExecuting,
                TriggerNotificationViaTopicCommand.IsExecuting)
            .ToPropertyEx(this, x => x.IsInProgress)
            .DisposeWith(Disposables);
    }

    public extern bool IsInProgress { [ObservableAsProperty] get; }

    public ReactiveCommand<Unit, Unit> CheckIfValidCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> SubscribeToTopicCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> UnsubscribeFromTopicCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> TriggerNotificationViaTokenCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> TriggerNotificationViaTopicCommand { get; private set; }
}
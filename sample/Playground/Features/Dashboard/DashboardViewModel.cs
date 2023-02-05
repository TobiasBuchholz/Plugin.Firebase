using Playground.Common.Services.DynamicLink;
using Playground.Common.Services.PushNotification;

namespace Playground.Features.Dashboard;

[Preserve(AllMembers = true)]
public sealed class DashboardViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IUserInteractionService _userInteractionService;
    
    public DashboardViewModel(
        // IDynamicLinkService dynamicLinkService,
        INavigationService navigationService,
        IPushNotificationService pushNotificationService,
        IUserInteractionService userInteractionService)
    {
        _navigationService = navigationService;
        _pushNotificationService = pushNotificationService;
        _userInteractionService = userInteractionService;
    
        InitCommands();
        HandleTappedPushNotification();
        // dynamicLinkService.Register();
    }

    private void InitCommands()
    {
        NavigateToAuthPageCommand = ReactiveCommand.CreateFromTask(() => _navigationService.GoToAsync(NavigationPaths.ToAuthPage()));
        NavigateToCloudMessagingPageCommand = ReactiveCommand.CreateFromTask(() => _navigationService.GoToAsync(NavigationPaths.ToCloudMessagingPage()));
        NavigateToRemoteConfigPageCommand = ReactiveCommand.CreateFromTask(() => _navigationService.GoToAsync(NavigationPaths.ToRemoteConfigPage()));
        NavigateToStoragePageCommand = ReactiveCommand.CreateFromTask(() => _navigationService.GoToAsync(NavigationPaths.ToStoragePage()));
    }

    private void HandleTappedPushNotification()
    {
        _pushNotificationService
            .NotificationTapped
            .Subscribe(x => _userInteractionService.ShowDefaultDialogAsync(x.Title, x.Body))
            .DisposeWith(Disposables);
    }

    public ReactiveCommand<Unit, Unit> NavigateToAuthPageCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> NavigateToCloudMessagingPageCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> NavigateToRemoteConfigPageCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> NavigateToStoragePageCommand { get; private set; }
}
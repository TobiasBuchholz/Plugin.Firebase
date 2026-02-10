using Plugin.Firebase.AppCheck;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Playground.Common.Services.Navigation;
using Playground.Common.Services.Preferences;
using Playground.Common.Services.UserInteraction;

namespace Playground.Features.AppCheck;

[Preserve(AllMembers = true)]
public sealed class AppCheckViewModel : ViewModelBase
{
    private readonly ISchedulerService _schedulerService;
    private readonly IUserInteractionService _userInteractionService;
    private readonly INavigationService _navigationService;
    private readonly IPreferencesService _preferencesService;
    private readonly IFirebaseAppCheck _firebaseAppCheck;

    public AppCheckViewModel(
        ISchedulerService schedulerService,
        IUserInteractionService userInteractionService,
        INavigationService navigationService,
        IPreferencesService preferencesService,
        IFirebaseAppCheck firebaseAppCheck,
        AppCheckOptions configuredOptions)
    {
        _schedulerService = schedulerService;
        _userInteractionService = userInteractionService;
        _navigationService = navigationService;
        _preferencesService = preferencesService;
        _firebaseAppCheck = firebaseAppCheck;
        ConfiguredProvider = configuredOptions.Provider.ToString();
        InitCommands();
        StatusMessage = $"Configured provider: {ConfiguredProvider}";
        CurrentToken = "No token fetched yet.";
    }

    private void InitCommands()
    {
        FetchTokenCommand = ReactiveCommand.CreateFromTask(FetchTokenAsync);
        ResetModeCommand = ReactiveCommand.CreateFromTask(ResetModeAsync);
        var canCopyToken = this
            .WhenAnyValue(x => x.CurrentToken)
            .Select(token => !string.IsNullOrWhiteSpace(token) && token != "No token fetched yet.");
        CopyTokenCommand = ReactiveCommand.CreateFromTask(CopyTokenAsync, canCopyToken);

        Observable
            .Merge(
                FetchTokenCommand.ThrownExceptions,
                CopyTokenCommand.ThrownExceptions,
                ResetModeCommand.ThrownExceptions)
            .LogThrownException()
            .Select(ex => $"AppCheck error: {ex.Message}")
            .ObserveOn(_schedulerService.Main)
            .Subscribe(message => StatusMessage = message)
            .DisposeWith(Disposables);
    }

    private async Task FetchTokenAsync()
    {
        var token = await _firebaseAppCheck.GetTokenAsync(true);
        CurrentToken = token;
        StatusMessage = $"Token fetched ({token.Length} chars). Tap token text to copy.";
    }

    private async Task CopyTokenAsync()
    {
        await Clipboard.Default.SetTextAsync(CurrentToken);
        await _userInteractionService.ShowDefaultSnackbarAsync("App Check token copied.");
    }

    private async Task ResetModeAsync()
    {
        var userInfo = new UserInfoBuilder()
            .WithTitle("Change App Check Mode")
            .WithMessage("Select a different App Check provider. The new configuration will be applied when you restart the app.")
            .WithDefaultButton("Select Mode")
            .WithCancelButton("Cancel")
            .As(UserInfoType.ActionSheet)
            .Build();

        var result = await _userInteractionService.ShowAsActionSheetAsync(userInfo);

        if(result == 0) // "Select Mode" button
        {
            // Navigate to mode selection page
            await _navigationService.GoToAsync(NavigationPaths.ToAppCheckModeSelectionPage());
        }
    }

    [Reactive]
    public string StatusMessage { get; private set; }
    [Reactive]
    public string ConfiguredProvider { get; private set; }
    [Reactive]
    public string CurrentToken { get; private set; }

    public ReactiveCommand<Unit, Unit> FetchTokenCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> CopyTokenCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ResetModeCommand { get; private set; }
}
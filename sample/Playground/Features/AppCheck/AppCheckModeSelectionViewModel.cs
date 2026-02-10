using Plugin.Firebase.AppCheck;
using Playground.Common.Services.Navigation;
using Playground.Common.Services.Preferences;
using Playground.Common.Services.UserInteraction;

namespace Playground.Features.AppCheck;

[Preserve(AllMembers = true)]
public sealed class AppCheckModeSelectionViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IPreferencesService _preferencesService;
    private readonly IUserInteractionService _userInteractionService;
    private readonly ISchedulerService _schedulerService;

    public AppCheckModeSelectionViewModel(
        INavigationService navigationService,
        IPreferencesService preferencesService,
        IUserInteractionService userInteractionService,
        ISchedulerService schedulerService)
    {
        _navigationService = navigationService;
        _preferencesService = preferencesService;
        _userInteractionService = userInteractionService;
        _schedulerService = schedulerService;
        InitCommands();
        HeaderText = "Select App Check Mode\n\nChoose an App Check provider to test.\n\nClose and restart the app to apply the new configuration.";
    }

    private void InitCommands()
    {
        SelectModeCommand = ReactiveCommand.CreateFromTask<string>(SelectModeAsync);
        SelectModeCommand.ThrownExceptions
            .LogThrownException()
            .Select(ex => $"Error: {ex.Message}")
            .ObserveOn(_schedulerService.Main)
            .Subscribe(message => StatusMessage = message)
            .DisposeWith(Disposables);
    }

    private async Task SelectModeAsync(string mode)
    {
        try {
            _preferencesService.Set(PreferenceKeys.AppCheckMode, mode);

            // Show info dialog - user must acknowledge and close app manually
            var userInfo = new UserInfoBuilder()
                .WithTitle("Mode Saved - Restart Required")
                .WithMessage($"Mode '{mode}' has been saved.\n\n⚠️ You MUST close and restart the app manually to apply the new App Check configuration.\n\nFirebase can only be configured once per app session.\n\nTap 'I Understand' then close the app.")
                .WithDefaultButton("I Understand")
                .As(UserInfoType.Dialog)
                .Build();

            await _userInteractionService.ShowAsDialogAsync(userInfo);

            // Navigate back
            await _navigationService.PopAsync();
        } catch(Exception ex) {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [Reactive]
    public string HeaderText { get; private set; }

    [Reactive]
    public string StatusMessage { get; private set; }

    public ReactiveCommand<string, Unit> SelectModeCommand { get; private set; }
}
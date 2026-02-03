using Plugin.Firebase.AppCheck;

namespace Playground.Features.AppCheck;

[Preserve(AllMembers = true)]
public sealed class AppCheckViewModel : ViewModelBase
{
    private readonly ISchedulerService _schedulerService;

    public AppCheckViewModel(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
        InitCommands();
        StatusMessage = "Tap a provider to configure Firebase AppCheck.";
    }

    private void InitCommands()
    {
        ConfigureDisabledCommand = ReactiveCommand.CreateFromTask(() => ConfigureAsync(AppCheckOptions.Disabled));
        ConfigureDebugCommand = ReactiveCommand.CreateFromTask(() => ConfigureAsync(AppCheckOptions.Debug));
        ConfigureDeviceCheckCommand = ReactiveCommand.CreateFromTask(() => ConfigureAsync(AppCheckOptions.DeviceCheck));
        ConfigureAppAttestCommand = ReactiveCommand.CreateFromTask(() => ConfigureAsync(AppCheckOptions.AppAttest));
        ConfigurePlayIntegrityCommand = ReactiveCommand.CreateFromTask(() => ConfigureAsync(AppCheckOptions.PlayIntegrity));

        Observable
            .Merge(
                ConfigureDisabledCommand.ThrownExceptions,
                ConfigureDebugCommand.ThrownExceptions,
                ConfigureDeviceCheckCommand.ThrownExceptions,
                ConfigureAppAttestCommand.ThrownExceptions,
                ConfigurePlayIntegrityCommand.ThrownExceptions)
            .LogThrownException()
            .Select(ex => $"AppCheck configuration failed: {ex.Message}")
            .ObserveOn(_schedulerService.Main)
            .Subscribe(message => StatusMessage = message)
            .DisposeWith(Disposables);
    }

    private Task ConfigureAsync(AppCheckOptions options)
    {
        return Observable
            .Start(() => CrossFirebaseAppCheck.Configure(options), _schedulerService.Main)
            .Do(_ => StatusMessage = $"AppCheck configured: {options.Provider}")
            .ToTask();
    }

    [Reactive]
    public string StatusMessage { get; private set; }

    public ReactiveCommand<Unit, Unit> ConfigureDisabledCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ConfigureDebugCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ConfigureDeviceCheckCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ConfigureAppAttestCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ConfigurePlayIntegrityCommand { get; private set; }
}
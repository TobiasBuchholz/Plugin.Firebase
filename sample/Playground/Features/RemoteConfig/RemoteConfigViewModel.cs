using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Playground.Common.Base;
using Playground.Common.Services.UserInteraction;
using Playground.Resources;
using Plugin.Firebase.RemoteConfig;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Playground.Features.RemoteConfig
{
    public sealed class RemoteConfigViewModel : ViewModelBase
    {
        private const string RemoteConfigKey = "some_remote_config_key";

        private readonly IUserInteractionService _userInteractionService;
        private readonly IFirebaseRemoteConfig _firebaseRemoteConfig;
        
        public RemoteConfigViewModel(
            IUserInteractionService userInteractionService,
            IFirebaseRemoteConfig firebaseRemoteConfig)
        {
            _userInteractionService = userInteractionService;
            _firebaseRemoteConfig = firebaseRemoteConfig;

            SetRemoteConfigDefaultsAsync();
            InitCommands();
            InitProperties();
        }

        private Task SetRemoteConfigDefaultsAsync()
        {
            return _firebaseRemoteConfig.SetDefaultsAsync((RemoteConfigKey, "some default value"));
        }

        private void InitCommands()
        {
            FetchAndActivateCommand = ReactiveCommand.CreateFromTask(FetchAndActivateAsync);

            FetchAndActivateCommand
                .ThrownExceptions
                .LogThrownException()
                .Subscribe(e => _userInteractionService.ShowErrorDialogAsync(Localization.DialogTitleUnexpectedError, e))
                .DisposeWith(Disposables);
        }

        private async Task FetchAndActivateAsync()
        {
            await _firebaseRemoteConfig.SetRemoteConfigSettingsAsync(new RemoteConfigSettings(minimumFetchInterval:TimeSpan.Zero));
            await _firebaseRemoteConfig.FetchAndActivateAsync();
        }

        private void InitProperties()
        {
            this.WhenAnyObservable(x => x.FetchAndActivateCommand)
                .Select(_ => _firebaseRemoteConfig.GetString(RemoteConfigKey))
                .StartWith(_firebaseRemoteConfig.GetString(RemoteConfigKey))
                .ToPropertyEx(this, x => x.SomeRemoteConfigValue)
                .DisposeWith(Disposables);
        }

        public extern string SomeRemoteConfigValue { [ObservableAsProperty] get; }
        public ReactiveCommand<Unit, Unit> FetchAndActivateCommand { get; private set; }
    }
}
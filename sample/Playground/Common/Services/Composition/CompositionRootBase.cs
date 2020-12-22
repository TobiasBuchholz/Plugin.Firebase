using System;
using System.Collections.Generic;
using System.Linq;
using Playground.Common.Base;
using Playground.Common.Services.Auth;
using Playground.Common.Services.DynamicLink;
using Playground.Common.Services.Navigation;
using Playground.Common.Services.Preferences;
using Playground.Common.Services.Scheduler;
using Playground.Common.Services.UserInteraction;
using Playground.Features.Auth;
using Playground.Features.Dashboard;
using Playground.Features.RemoteConfig;
using Playground.Features.Storage;
using Plugin.Firebase.Auth;
using Plugin.Firebase.DynamicLinks;
using Plugin.Firebase.RemoteConfig;
using Plugin.Firebase.Storage;
using ReactiveUI;

namespace Playground.Common.Services.Composition
{
    public abstract class CompositionRootBase
    {
        private readonly Lazy<IAuthService> _authService;
        private readonly Lazy<INavigationService> _navigationService;
        private readonly Lazy<IPreferencesService> _preferencesService;
        protected readonly Lazy<ISchedulerService> _schedulerService;
        private readonly Lazy<IUserInteractionService> _userInteractionService;

        private readonly Lazy<IFirebaseAuth> _firebaseAuth;
        private readonly Lazy<IFirebaseStorage> _firebaseStorage;
        private readonly Lazy<IFirebaseRemoteConfig> _firebaseRemoteConfig;

        protected CompositionRootBase()
        {
            _authService = new Lazy<IAuthService>(CreateAuthService);
            _navigationService = new Lazy<INavigationService>(CreateNavigationService);
            _preferencesService = new Lazy<IPreferencesService>(CreatePreferencesService);
            _schedulerService = new Lazy<ISchedulerService>(CreateSchedulerService);
            _userInteractionService = new Lazy<IUserInteractionService>(CreateUserInteractionService);
            
            _firebaseAuth = new Lazy<IFirebaseAuth>(CreateFirebaseAuth);
            _firebaseStorage = new Lazy<IFirebaseStorage>(CreateFirebaseStorage);
            _firebaseRemoteConfig = new Lazy<IFirebaseRemoteConfig>(CreateFirebaseRemoteConfig);

            RegisterDynamicLinks();
        }
        
        private IAuthService CreateAuthService() =>
            new AuthService(
                _firebaseAuth.Value,
                _preferencesService.Value);

        private INavigationService CreateNavigationService() =>
            new NavigationService(_schedulerService.Value.Main);
        
        private static IPreferencesService CreatePreferencesService() =>
            new PreferencesService();
        
        private static ISchedulerService CreateSchedulerService() =>
            new SchedulerService();

        protected abstract IUserInteractionService CreateUserInteractionService();

        private static IFirebaseAuth CreateFirebaseAuth() =>
            CrossFirebaseAuth.Current;
        
        private static IFirebaseStorage CreateFirebaseStorage() =>
            CrossFirebaseStorage.Current;
        
        private static IFirebaseRemoteConfig CreateFirebaseRemoteConfig() =>
            CrossFirebaseRemoteConfig.Current;
        
        public ViewModelBase ResolveViewModel(IViewFor viewFor, IEnumerable<object> parameters = null) 
        {
            var paramList = parameters?.ToList();
            switch(viewFor) {
                case DashboardPage _:
                    return new DashboardViewModel(
                        _navigationService.Value);
                case AuthPage _:
                    return new AuthViewModel(
                        _authService.Value,
                        _userInteractionService.Value,
                        _schedulerService.Value);
                case RemoteConfigPage _:
                    return new RemoteConfigViewModel(
                        _userInteractionService.Value,
                        _firebaseRemoteConfig.Value);
                case StoragePage _:
                    return new StorageViewModel(
                        _userInteractionService.Value,
                        _firebaseStorage.Value);
            }
            throw new ArgumentException($"Couldn't resolve corresponding viewmodel for IViewFor: {viewFor.GetType()}");
        }

        public ISchedulerService ResolveSchedulerService() =>
            _schedulerService.Value;

        private void RegisterDynamicLinks()
        {
            new DynamicLinkService(
                    CrossFirebaseDynamicLinks.Current,
                    _authService.Value,
                    _navigationService.Value,
                    _preferencesService.Value,
                    _userInteractionService.Value,
                    _schedulerService.Value)
                .Register();
        }
    }
}
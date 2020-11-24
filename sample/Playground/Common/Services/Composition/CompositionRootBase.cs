using System;
using System.Collections.Generic;
using System.Linq;
using Playground.Common.Base;
using Playground.Common.Services.Navigation;
using Playground.Common.Services.Scheduler;
using Playground.Common.Services.UserInteraction;
using Playground.Features.Dashboard;
using Playground.Features.RemoteConfig;
using Playground.Features.Storage;
using Plugin.Firebase.RemoteConfig;
using Plugin.Firebase.Storage;
using ReactiveUI;

namespace Playground.Common.Services.Composition
{
    public abstract class CompositionRootBase
    {
        private readonly Lazy<INavigationService> _navigationService;
        protected readonly Lazy<ISchedulerService> _schedulerService;
        private readonly Lazy<IUserInteractionService> _userInteractionService;
        private readonly Lazy<IFirebaseStorage> _firebaseStorage;
        private readonly Lazy<IFirebaseRemoteConfig> _firebaseRemoteConfig;

        protected CompositionRootBase()
        {
            _navigationService = new Lazy<INavigationService>(CreateNavigationService);
            _schedulerService = new Lazy<ISchedulerService>(CreateSchedulerService);
            _userInteractionService = new Lazy<IUserInteractionService>(CreateUserInteractionService);
            _firebaseStorage = new Lazy<IFirebaseStorage>(CreateFirebaseStorage);
            _firebaseRemoteConfig = new Lazy<IFirebaseRemoteConfig>(CreateFirebaseRemoteConfig);
        }

        private INavigationService CreateNavigationService() =>
            new NavigationService(_schedulerService.Value.Main);
        
        private static ISchedulerService CreateSchedulerService() =>
            new SchedulerService();

        protected abstract IUserInteractionService CreateUserInteractionService();
        
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
                case RemoteConfigPage _:
                    return new RemoteConfigViewModel(
                        _userInteractionService.Value,
                        _firebaseRemoteConfig.Value);
                case StoragePage _:
                    return new StorageViewModel(
                        _firebaseStorage.Value);
            }
            throw new ArgumentException($"Couldn't resolve corresponding viewmodel for IViewFor: {viewFor.GetType()}");
        }

        public ISchedulerService ResolveSchedulerService() =>
            _schedulerService.Value;
    }
}
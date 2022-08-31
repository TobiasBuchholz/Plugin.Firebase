using Genesis.Logging;
using Playground.Common.Base;
using Playground.Common.Services.Auth;
using Playground.Common.Services.DynamicLink;
using Playground.Common.Services.Navigation;
using Playground.Common.Services.Preferences;
using Playground.Common.Services.PushNotification;
using Playground.Common.Services.Scheduler;
using Playground.Common.Services.UserInteraction;
using Playground.Features.Auth;
using Playground.Features.CloudMessaging;
using Playground.Features.Dashboard;
using Playground.Features.RemoteConfig;
using Playground.Features.Storage;
using Plugin.Firebase.Auth;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.DynamicLinks;
using Plugin.Firebase.Functions;
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
        private readonly Lazy<IPushNotificationService> _pushNotificationService;
        protected readonly Lazy<ISchedulerService> _schedulerService;
        private readonly Lazy<IUserInteractionService> _userInteractionService;

        private readonly Lazy<IFirebaseAuth> _firebaseAuth;
        private readonly Lazy<IFirebaseCloudMessaging> _firebaseCloudMessaging;
        private readonly Lazy<IFirebaseFunctions> _firebaseFunctions;
        private readonly Lazy<IFirebaseStorage> _firebaseStorage;
        private readonly Lazy<IFirebaseRemoteConfig> _firebaseRemoteConfig;

        protected CompositionRootBase()
        {
            _authService = new Lazy<IAuthService>(CreateAuthService);
            _navigationService = new Lazy<INavigationService>(CreateNavigationService);
            _preferencesService = new Lazy<IPreferencesService>(CreatePreferencesService);
            _pushNotificationService = new Lazy<IPushNotificationService>(CreatePushNotificationService);
            _schedulerService = new Lazy<ISchedulerService>(CreateSchedulerService);
            _userInteractionService = new Lazy<IUserInteractionService>(CreateUserInteractionService);

            _firebaseAuth = new Lazy<IFirebaseAuth>(CreateFirebaseAuth);
            _firebaseCloudMessaging = new Lazy<IFirebaseCloudMessaging>(CreateFirebaseCloudMessaging);
            _firebaseFunctions = new Lazy<IFirebaseFunctions>(CreateFirebaseFunctions);
            _firebaseStorage = new Lazy<IFirebaseStorage>(CreateFirebaseStorage);
            _firebaseRemoteConfig = new Lazy<IFirebaseRemoteConfig>(CreateFirebaseRemoteConfig);

            RegisterDynamicLinks();
        }

        private IAuthService CreateAuthService() =>
            new AuthService(
                _firebaseAuth.Value,
                _preferencesService.Value);

        private INavigationService CreateNavigationService() =>
            new NavigationService(_schedulerService.Value);

        private static IPreferencesService CreatePreferencesService() =>
            new PreferencesService();

        private IPushNotificationService CreatePushNotificationService() =>
            new PushNotificationService(
                _firebaseCloudMessaging.Value,
                _firebaseFunctions.Value,
                LoggerService.GetLogger(nameof(PushNotificationService)));

        private static ISchedulerService CreateSchedulerService() =>
            new SchedulerService();

        protected abstract IUserInteractionService CreateUserInteractionService();

        private static IFirebaseAuth CreateFirebaseAuth() =>
            CrossFirebaseAuth.Current;

        private static IFirebaseCloudMessaging CreateFirebaseCloudMessaging() =>
            CrossFirebaseCloudMessaging.Current;

        private static IFirebaseFunctions CreateFirebaseFunctions() =>
            CrossFirebaseFunctions.Current;

        private static IFirebaseStorage CreateFirebaseStorage() =>
            CrossFirebaseStorage.Current;

        private static IFirebaseRemoteConfig CreateFirebaseRemoteConfig() =>
            CrossFirebaseRemoteConfig.Current;

        // public ViewModelBase ResolveViewModel(IViewFor viewFor, IEnumerable<object> parameters = null)
        // {
        //     var paramList = parameters?.ToList();
        //     switch(viewFor) {
        //         case DashboardPage _:
        //             return new DashboardViewModel(
        //                 _navigationService.Value,
        //                 _pushNotificationService.Value,
        //                 _userInteractionService.Value);
        //         case AuthPage _:
        //             return new AuthViewModel(
        //                 _authService.Value,
        //                 _userInteractionService.Value,
        //                 _schedulerService.Value);
        //         case CloudMessagingPage _:
        //             return new CloudMessagingViewModel(
        //                 _pushNotificationService.Value,
        //                 _userInteractionService.Value);
        //         case RemoteConfigPage _:
        //             return new RemoteConfigViewModel(
        //                 _userInteractionService.Value,
        //                 _firebaseRemoteConfig.Value);
        //         case StoragePage _:
        //             return new StorageViewModel(
        //                 _userInteractionService.Value,
        //                 _firebaseStorage.Value);
        //     }
        //     throw new ArgumentException($"Couldn't resolve corresponding viewmodel for IViewFor: {viewFor.GetType()}");
        // }

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
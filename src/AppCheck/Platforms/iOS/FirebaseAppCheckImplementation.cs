using Firebase.AppCheck;
using Firebase.Core;
using Foundation;
using ObjCRuntime;
using Plugin.Firebase.Core;

namespace Plugin.Firebase.AppCheck;

public sealed class FirebaseAppCheckImplementation : IFirebaseAppCheck
{
    private readonly object _syncRoot = new();
    private AppCheckOptions _options = AppCheckOptions.Disabled;
    private IDisposable _beforeConfigureRegistration;

    public void Configure(AppCheckOptions options)
    {
        if(options == null) {
            throw new ArgumentNullException(nameof(options));
        }

        lock(_syncRoot) {
            _options = options;
        }

        if(options.Provider == AppCheckProviderType.Disabled) {
            _beforeConfigureRegistration?.Dispose();
            _beforeConfigureRegistration = null;
            return;
        }

        _beforeConfigureRegistration ??= FirebaseInitializationHooks.RegisterBeforeConfigure(InstallProviderFactory);
    }

    private void InstallProviderFactory()
    {
        AppCheckOptions options;
        lock(_syncRoot) {
            options = _options;
        }

        if(options.Provider == AppCheckProviderType.Disabled) {
            return;
        }

        switch(options.Provider) {
            case AppCheckProviderType.Debug:
                var debugFactory = new AppCheckDebugProviderFactory();
                global::Firebase.AppCheck.AppCheck.SetAppCheckProviderFactory(debugFactory);
                break;

            case AppCheckProviderType.DeviceCheck:
                var deviceCheckFactory = new DeviceCheckProviderFactory();
                global::Firebase.AppCheck.AppCheck.SetAppCheckProviderFactory(deviceCheckFactory);
                break;

            case AppCheckProviderType.AppAttest:
                var appAttestAdapter = new AppAttestProviderFactoryAdapter();
                global::Firebase.AppCheck.AppCheck.SetAppCheckProviderFactory(appAttestAdapter);
                break;

            case AppCheckProviderType.PlayIntegrity:
                throw new NotSupportedException("AppCheck Play Integrity provider is not supported on iOS.");

            default:
                break;
        }
    }

    public Task<string> GetTokenAsync(bool forceRefresh = false)
    {
        var taskCompletionSource = new TaskCompletionSource<string>();

        global::Firebase.AppCheck.AppCheck.SharedInstance.TokenForcingRefresh(forceRefresh, (token, error) => {
            if(error != null) {
                taskCompletionSource.TrySetException(new NSErrorException(error));
                return;
            }

            var rawToken = token?.Token;
            if(string.IsNullOrWhiteSpace(rawToken)) {
                taskCompletionSource.TrySetException(new InvalidOperationException("Firebase AppCheck returned an empty iOS token."));
                return;
            }

            taskCompletionSource.TrySetResult(rawToken);
        });

        return taskCompletionSource.Task;
    }

    public void Dispose()
    {
        _beforeConfigureRegistration?.Dispose();
        _beforeConfigureRegistration = null;
    }

    private sealed class AppAttestProviderFactoryAdapter : NSObject, IAppCheckProviderFactory
    {
        public IAppCheckProvider CreateProviderWithApp(App app)
        {
            if(!OperatingSystem.IsIOSVersionAtLeast(14)) {
                return null;
            }

            return (IAppCheckProvider) new AppAttestProvider(app);
        }
    }
}
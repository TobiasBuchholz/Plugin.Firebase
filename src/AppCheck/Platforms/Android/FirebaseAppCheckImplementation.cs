using Firebase.AppCheck;
using Firebase.AppCheck.Debug;
using Firebase.AppCheck.PlayIntegrity;
using Plugin.Firebase.Core;

namespace Plugin.Firebase.AppCheck;

public sealed class FirebaseAppCheckImplementation : IFirebaseAppCheck
{
    private readonly object _syncRoot = new();
    private AppCheckOptions _options = AppCheckOptions.Disabled;
    private IDisposable _afterInitializeRegistration;

    public void Configure(AppCheckOptions options)
    {
        if(options == null) {
            throw new ArgumentNullException(nameof(options));
        }

        lock(_syncRoot) {
            _options = options;
        }

        if(options.Provider == AppCheckProviderType.Disabled) {
            _afterInitializeRegistration?.Dispose();
            _afterInitializeRegistration = null;
            return;
        }

        _afterInitializeRegistration ??= FirebaseInitializationHooks.RegisterAfterInitialize(InstallProviderFactory);
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

        IAppCheckProviderFactory factory = null;
        switch(options.Provider) {
            case AppCheckProviderType.Debug:
                factory = (IAppCheckProviderFactory) DebugAppCheckProviderFactory.Instance;
                break;
            case AppCheckProviderType.PlayIntegrity:
                factory = (IAppCheckProviderFactory) PlayIntegrityAppCheckProviderFactory.Instance;
                break;
            case AppCheckProviderType.DeviceCheck:
            case AppCheckProviderType.AppAttest:
                throw new NotSupportedException($"AppCheck provider '{options.Provider}' is not supported on Android.");
        }

        if(factory != null) {
            var firebaseApp = global::Firebase.FirebaseApp.Instance;
            global::Firebase.AppCheck.FirebaseAppCheck.GetInstance(firebaseApp)
                .InstallAppCheckProviderFactory(factory);
        }
    }

    public void Dispose()
    {
        _afterInitializeRegistration?.Dispose();
        _afterInitializeRegistration = null;
    }
}
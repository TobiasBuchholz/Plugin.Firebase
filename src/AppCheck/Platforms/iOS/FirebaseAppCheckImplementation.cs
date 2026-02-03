using Firebase.AppCheck;
using Firebase.Core;
using Foundation;
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

        IAppCheckProviderFactory providerFactory = options.Provider switch {
            AppCheckProviderType.Debug => (IAppCheckProviderFactory) new AppCheckDebugProviderFactory(),
            AppCheckProviderType.DeviceCheck => (IAppCheckProviderFactory) new DeviceCheckProviderFactory(),
            AppCheckProviderType.AppAttest => new AppAttestProviderFactoryAdapter(),
            AppCheckProviderType.PlayIntegrity => throw new NotSupportedException("AppCheck Play Integrity provider is not supported on iOS."),
            _ => null
        };

        if(providerFactory != null) {
            global::Firebase.AppCheck.AppCheck.SetAppCheckProviderFactory(providerFactory);
        }
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
                throw new NotSupportedException("AppCheck App Attest provider requires iOS 14 or newer.");
            }

            return (IAppCheckProvider) new AppAttestProvider(app);
        }
    }
}
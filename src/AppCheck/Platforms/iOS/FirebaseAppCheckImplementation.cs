#if FIREBASE_APP_CHECK_IOS
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

        if(options.Mode == AppCheckMode.Disabled) {
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

        if(options.Mode == AppCheckMode.Disabled) {
            return;
        }

        AppCheckProviderFactory factory = options.Mode switch
        {
            AppCheckMode.Debug => new AppCheckDebugProviderFactory(),
            AppCheckMode.Production => new AppAttestOrDeviceCheckProviderFactory(),
            _ => null
        };

        if(factory != null) {
            AppCheck.SetAppCheckProviderFactory(factory);
        }
    }

    public void Dispose()
    {
        _beforeConfigureRegistration?.Dispose();
        _beforeConfigureRegistration = null;
    }

    private sealed class AppAttestOrDeviceCheckProviderFactory : NSObject, AppCheckProviderFactory
    {
        public AppCheckProvider CreateProviderWithApp(App app)
        {
            if(OperatingSystem.IsIOSVersionAtLeast(14)) {
                return new AppAttestProvider(app);
            }

            return new DeviceCheckProvider(app);
        }
    }
}
#endif
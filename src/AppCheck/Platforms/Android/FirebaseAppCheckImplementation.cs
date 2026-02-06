using Android.Gms.Extensions;
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

        _afterInitializeRegistration ??= FirebaseInitializationHooks.RegisterAfterInitialize(
            InstallProviderFactory
        );
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

        global::Firebase.FirebaseApp firebaseApp;
        try {
            firebaseApp = global::Firebase.FirebaseApp.Instance;
        } catch(Java.Lang.IllegalStateException) {
            Console.WriteLine(
                "[Plugin.Firebase.AppCheck] Skipping provider installation: Firebase default app not initialized. "
                    + "Check your google-services.json or provide explicit FirebaseOptions to CrossFirebase.Initialize()."
            );
            return;
        }

        Console.WriteLine(
            $"Plugin.Firebase AppCheck: installing provider factory '{options.Provider}' (Android)."
        );

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
                throw new NotSupportedException(
                    $"AppCheck provider '{options.Provider}' is not supported on Android."
                );
        }

        if(factory != null) {
            global::Firebase
                .AppCheck.FirebaseAppCheck.GetInstance(firebaseApp)
                .InstallAppCheckProviderFactory(factory);
        }
    }

    public async Task<string> GetTokenAsync(bool forceRefresh = false)
    {
        var firebaseApp = global::Firebase.FirebaseApp.Instance;
        var tokenResult =
            await global::Firebase
                .AppCheck.FirebaseAppCheck.GetInstance(firebaseApp)
                .GetAppCheckToken(forceRefresh) as AppCheckToken;
        var rawToken = tokenResult?.Token;

        if(string.IsNullOrWhiteSpace(rawToken)) {
            throw new InvalidOperationException(
                "Firebase AppCheck returned an empty Android token."
            );
        }

        return rawToken;
    }

    public void Dispose()
    {
        _afterInitializeRegistration?.Dispose();
        _afterInitializeRegistration = null;
    }
}
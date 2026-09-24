using Android.Gms.Extensions;
using Firebase.AppCheck;
using Firebase.AppCheck.Debug;
using Firebase.AppCheck.PlayIntegrity;

namespace Plugin.Firebase.AppCheck;

public sealed class FirebaseAppCheckImplementation : IFirebaseAppCheck
{
    // The native provider factory is process-wide and cannot be removed, so every implementation instance,
    // including one created after CrossFirebaseAppCheck.Dispose(), shares the same installation state.
    private static readonly AppCheckProviderInstaller ProviderInstaller = new(InstallProviderFactory);

    public void Configure(AppCheckOptions options)
    {
        if(options == null) {
            throw new ArgumentNullException(nameof(options));
        }

        if(options.Provider is AppCheckProviderType.DeviceCheck or AppCheckProviderType.AppAttest) {
            throw new NotSupportedException(
                $"AppCheck provider '{options.Provider}' is not supported on Android."
            );
        }

        ProviderInstaller.Configure(options);
    }

    private static bool InstallProviderFactory(AppCheckProviderType provider)
    {
        global::Firebase.FirebaseApp firebaseApp;
        try {
            firebaseApp = global::Firebase.FirebaseApp.Instance;
        } catch(Java.Lang.IllegalStateException) {
            Console.WriteLine(
                "[Plugin.Firebase.AppCheck] Skipping provider installation: Firebase default app not initialized. "
                    + "Check your google-services.json or provide explicit FirebaseOptions to CrossFirebase.Initialize()."
            );
            return false;
        }

        Console.WriteLine(
            $"Plugin.Firebase AppCheck: installing provider factory '{provider}' (Android)."
        );

        IAppCheckProviderFactory factory = provider switch {
            AppCheckProviderType.Debug => (IAppCheckProviderFactory) DebugAppCheckProviderFactory.Instance,
            AppCheckProviderType.PlayIntegrity => (IAppCheckProviderFactory) PlayIntegrityAppCheckProviderFactory.Instance,
            _ => throw new NotSupportedException($"AppCheck provider '{provider}' is not supported on Android.")
        };

        global::Firebase
            .AppCheck.FirebaseAppCheck.GetInstance(firebaseApp)
            .InstallAppCheckProviderFactory(factory);
        return true;
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
        ProviderInstaller.CancelPendingInstallation();
    }
}
using Android.Gms.Extensions;
using Android.Runtime;
using Firebase.AppCheck;
using Firebase.AppCheck.Debug;
using Firebase.AppCheck.PlayIntegrity;

namespace Plugin.Firebase.AppCheck;

public sealed class FirebaseAppCheckImplementation : IFirebaseAppCheck
{
    // The configured provider applies to the default Firebase app rather than to one wrapper instance, so every
    // implementation instance, including one created after CrossFirebaseAppCheck.Dispose(), shares this state.
    private static readonly AppCheckProviderInstaller<global::Firebase.FirebaseApp> ProviderInstaller = new(
        () => Core.Platforms.Android.CrossFirebase.TryGetDefaultApp(out var app) ? app : null,
        InstallProviderFactory,
        (first, second) => JNIEnv.IsSameObject(first.Handle, second.Handle)
    );

    public void Configure(AppCheckOptions options)
    {
        if(options == null) {
            throw new ArgumentNullException(nameof(options));
        }

        if(options.Provider is not (AppCheckProviderType.Disabled or AppCheckProviderType.Debug or AppCheckProviderType.PlayIntegrity)) {
            throw new NotSupportedException(
                $"AppCheck provider '{options.Provider}' is not supported on Android."
            );
        }

        ProviderInstaller.Configure(options);
    }

    private static void InstallProviderFactory(
        global::Firebase.FirebaseApp firebaseApp,
        AppCheckProviderType provider
    )
    {
        Console.WriteLine(
            $"Plugin.Firebase AppCheck: installing provider factory '{provider}' (Android)."
        );

        IAppCheckProviderFactory factory = provider switch {
            AppCheckProviderType.Debug => (IAppCheckProviderFactory) DebugAppCheckProviderFactory.Instance,
            AppCheckProviderType.PlayIntegrity => (IAppCheckProviderFactory) PlayIntegrityAppCheckProviderFactory.Instance,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Configure only accepts Android providers.")
        };

        global::Firebase
            .AppCheck.FirebaseAppCheck.GetInstance(firebaseApp)
            .InstallAppCheckProviderFactory(factory);
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
        // The configured provider belongs to the default Firebase app, not to this instance, so disposing an
        // instance leaves it in place. Configure AppCheckOptions.Disabled before initialization to drop it.
    }
}
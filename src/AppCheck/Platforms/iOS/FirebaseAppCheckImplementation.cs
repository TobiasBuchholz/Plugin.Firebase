using Firebase.AppCheck;
using Firebase.Core;
using Foundation;
using Plugin.Firebase.Core;

namespace Plugin.Firebase.AppCheck;

/// <summary>
/// iOS implementation of the Firebase AppCheck service.
/// </summary>
public sealed class FirebaseAppCheckImplementation : IFirebaseAppCheck
{
    private readonly object _syncRoot = new();
    private AppCheckOptions _options = AppCheckOptions.Disabled;
    private IDisposable _beforeConfigureRegistration;

    /// <summary>
    /// Configures the Firebase AppCheck service with the specified options.
    /// </summary>
    /// <param name="options">The AppCheck configuration options.</param>
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

        Console.WriteLine($"Plugin.Firebase AppCheck: installing provider factory '{options.Provider}' (iOS).");

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

    /// <summary>
    /// Gets a Firebase AppCheck token, optionally forcing a refresh.
    /// </summary>
    /// <param name="forceRefresh">Whether to force a refresh of the token.</param>
    /// <returns>A task that resolves to the AppCheck token string.</returns>
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

    /// <summary>
    /// Disposes the AppCheck implementation and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        _beforeConfigureRegistration?.Dispose();
        _beforeConfigureRegistration = null;
    }

    private sealed class AppAttestProviderFactoryAdapter : NSObject, IAppCheckProviderFactory
    {
        public NSObject CreateProviderWithApp(App app)
        {
            if(!OperatingSystem.IsIOSVersionAtLeast(14)) {
                return null;
            }

            return new AppAttestProvider(app);
        }
    }
}

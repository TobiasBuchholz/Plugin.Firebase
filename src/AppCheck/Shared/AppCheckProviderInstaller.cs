using Plugin.Firebase.Core;

namespace Plugin.Firebase.AppCheck;

/// <summary>
/// Installs the configured App Check provider factory on the default Firebase app once Firebase is initialized,
/// for a native SDK that can replace an installed factory but cannot remove one (Android).
/// </summary>
/// <remarks>
/// Platform-neutral so the configuration rules can be unit tested without a device.
/// </remarks>
/// <typeparam name="TApp">The native Firebase app type. An installed factory belongs to one app instance.</typeparam>
internal sealed class AppCheckProviderInstaller<TApp> where TApp : class
{
    private readonly object _syncRoot = new();
    private readonly Func<TApp?> _getDefaultApp;
    private readonly Action<TApp, AppCheckProviderType> _installProviderFactory;
    private readonly Func<TApp, TApp, bool> _isSameApp;
    private int _registered;
    private AppCheckOptions _options = AppCheckOptions.Disabled;
    private bool _initialized;
    private (TApp App, AppCheckProviderType Provider)? _installed;

    /// <param name="getDefaultApp">Returns the default Firebase app, or <c>null</c> when there is none.</param>
    /// <param name="installProviderFactory">Installs the native factory for a provider on an app.</param>
    /// <param name="isSameApp">Returns whether two references point to the same native app instance.</param>
    public AppCheckProviderInstaller(
        Func<TApp?> getDefaultApp,
        Action<TApp, AppCheckProviderType> installProviderFactory,
        Func<TApp, TApp, bool> isSameApp)
    {
        _getDefaultApp = getDefaultApp;
        _installProviderFactory = installProviderFactory;
        _isSameApp = isSameApp;
    }

    /// <summary>
    /// Installs the provider now if Firebase is already initialized, otherwise once it is. Configuring the
    /// provider that is already installed does nothing.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="options"/> disables App Check while a provider factory is installed on the default app.
    /// </exception>
    public void Configure(AppCheckOptions options)
    {
        // Registered on first use rather than in the constructor, so a static installer never takes the hooks lock
        // during type initialization. Registered before the first initialization, the callback also runs after every
        // later CrossFirebase.Initialize(), so a re-created default app gets the configured provider.
        if(Interlocked.Exchange(ref _registered, 1) == 0) {
            _ = FirebaseInitializationHooks.RegisterAfterInitialize(OnFirebaseInitialized);
        }

        lock(_syncRoot) {
            if(options.Provider == AppCheckProviderType.Disabled && _installed != null) {
                DropRecordIfAppReplaced(_getDefaultApp());
                if(_installed is { } installed) {
                    throw new InvalidOperationException(
                        $"App Check cannot be disabled after the '{installed.Provider}' provider factory was installed on the "
                            + "default Firebase app, because the native Firebase SDK cannot remove an installed provider factory. "
                            + "It stays active until the app process restarts."
                    );
                }
            }

            var previousOptions = _options;
            _options = options;
            if(!_initialized) {
                return;
            }

            try {
                InstallConfiguredProvider();
            } catch {
                // Otherwise the next CrossFirebase.Initialize() would install the provider this call failed to install.
                _options = previousOptions;
                throw;
            }
        }
    }

    private void OnFirebaseInitialized()
    {
        lock(_syncRoot) {
            _initialized = true;
            InstallConfiguredProvider();
        }
    }

    // Callers hold _syncRoot.
    private void InstallConfiguredProvider()
    {
        var provider = _options.Provider;
        if(provider == AppCheckProviderType.Disabled) {
            return;
        }

        var app = _getDefaultApp();
        DropRecordIfAppReplaced(app);
        if(app == null) {
            Console.WriteLine(
                "[Plugin.Firebase.AppCheck] Skipping provider installation: Firebase default app not initialized."
            );
            return;
        }

        if(_installed?.Provider == provider) {
            return;
        }

        _installProviderFactory(app, provider);
        _installed = (app, provider);
    }

    // Callers hold _syncRoot. A factory belongs to the app it was installed on, and a deleted app never becomes the
    // default again, so once that app is no longer the default the record is dropped, which also releases the app.
    private void DropRecordIfAppReplaced(TApp? defaultApp)
    {
        if(_installed is { } installed && (defaultApp == null || !_isSameApp(installed.App, defaultApp))) {
            _installed = null;
        }
    }
}
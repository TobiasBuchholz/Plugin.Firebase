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
    private AppCheckOptions _options = AppCheckOptions.Disabled;
    private bool _initialized;
    private TApp? _appWithFactory;
    private AppCheckProviderType _installedProvider;

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

        // Registered once for the installer's lifetime. Registered before the first initialization, it also runs
        // after every later CrossFirebase.Initialize(), so a re-created default app gets the configured provider.
        _ = FirebaseInitializationHooks.RegisterAfterInitialize(OnFirebaseInitialized);
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
        lock(_syncRoot) {
            if(options.Provider == AppCheckProviderType.Disabled
                && GetInstalledProvider(_getDefaultApp()) is { } installedProvider) {
                throw new InvalidOperationException(
                    $"App Check cannot be disabled after the '{installedProvider}' provider factory was installed on the "
                        + "default Firebase app, because the native Firebase SDK cannot remove an installed provider factory. "
                        + "It stays active until the app process restarts."
                );
            }

            _options = options;
            if(_initialized) {
                InstallConfiguredProvider();
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
        if(app == null) {
            Console.WriteLine(
                "[Plugin.Firebase.AppCheck] Skipping provider installation: Firebase default app not initialized."
            );
            return;
        }

        if(GetInstalledProvider(app) == provider) {
            return;
        }

        _installProviderFactory(app, provider);
        _appWithFactory = app;
        _installedProvider = provider;
    }

    // A factory belongs to the app it was installed on, so it no longer counts once that app is deleted.
    private AppCheckProviderType? GetInstalledProvider(TApp? app)
    {
        return app != null && _appWithFactory != null && _isSameApp(_appWithFactory, app)
            ? _installedProvider
            : null;
    }
}
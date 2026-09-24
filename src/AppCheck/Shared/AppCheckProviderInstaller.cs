using Plugin.Firebase.Core;

namespace Plugin.Firebase.AppCheck;

/// <summary>
/// Installs the configured App Check provider factory once Firebase is initialized, for a native SDK that
/// can replace an installed factory but cannot remove one (Android).
/// </summary>
/// <remarks>
/// Platform-neutral so the configuration rules can be unit tested without a device.
/// </remarks>
internal sealed class AppCheckProviderInstaller
{
    private readonly object _syncRoot = new();
    private readonly Func<AppCheckProviderType, bool> _installProviderFactory;
    private AppCheckOptions _options = AppCheckOptions.Disabled;
    private AppCheckProviderType? _installedProvider;
    private IDisposable? _afterInitializeRegistration;

    /// <param name="installProviderFactory">
    /// Installs the native factory for a provider. Returns <c>false</c> when nothing was installed.
    /// </param>
    public AppCheckProviderInstaller(Func<AppCheckProviderType, bool> installProviderFactory)
    {
        _installProviderFactory = installProviderFactory;
    }

    /// <summary>
    /// Installs the provider now if Firebase is already initialized, otherwise once it is.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="options"/> disables App Check after a provider factory was installed.
    /// </exception>
    public void Configure(AppCheckOptions options)
    {
        lock(_syncRoot) {
            if(options.Provider == AppCheckProviderType.Disabled) {
                if(_installedProvider is { } installedProvider) {
                    throw new InvalidOperationException(
                        $"App Check cannot be disabled after the '{installedProvider}' provider factory was installed, "
                            + "because the native Firebase SDK cannot remove an installed provider factory. "
                            + "Configure AppCheckOptions.Disabled before CrossFirebase.Initialize(), or restart the app."
                    );
                }

                _options = options;
                CancelPendingInstallation();
                return;
            }

            _options = options;

            // After initialization, RegisterAfterInitialize runs the callback immediately instead of keeping it,
            // so every provider change has to register again to reach the native SDK.
            _afterInitializeRegistration?.Dispose();
            _afterInitializeRegistration = FirebaseInitializationHooks.RegisterAfterInitialize(
                InstallConfiguredProvider
            );
        }
    }

    /// <summary>
    /// Cancels an installation that is still waiting for Firebase initialization. An installed provider
    /// factory stays installed.
    /// </summary>
    public void CancelPendingInstallation()
    {
        lock(_syncRoot) {
            _afterInitializeRegistration?.Dispose();
            _afterInitializeRegistration = null;
        }
    }

    private void InstallConfiguredProvider()
    {
        lock(_syncRoot) {
            var provider = _options.Provider;
            if(provider == AppCheckProviderType.Disabled) {
                return;
            }

            if(_installProviderFactory(provider)) {
                _installedProvider = provider;
            }
        }
    }
}
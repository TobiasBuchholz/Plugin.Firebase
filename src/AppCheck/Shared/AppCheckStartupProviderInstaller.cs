namespace Plugin.Firebase.AppCheck;

/// <summary>
/// Sets the App Check provider factory for a native SDK that reads it only when an app is configured (iOS), and
/// rejects changes once the default Firebase app exists.
/// </summary>
/// <remarks>
/// Platform-neutral so the rules can be unit tested without a device.
/// </remarks>
internal sealed class AppCheckStartupProviderInstaller
{
    private readonly object _syncRoot = new();
    private readonly Func<bool> _isDefaultAppConfigured;
    private readonly Action<AppCheckProviderType> _setProviderFactory;
    private AppCheckProviderType? _installedProvider;

    /// <param name="isDefaultAppConfigured">Returns whether the native default Firebase app exists.</param>
    /// <param name="setProviderFactory">Sets the process-wide native provider factory for a provider.</param>
    public AppCheckStartupProviderInstaller(
        Func<bool> isDefaultAppConfigured,
        Action<AppCheckProviderType> setProviderFactory)
    {
        _isDefaultAppConfigured = isDefaultAppConfigured;
        _setProviderFactory = setProviderFactory;
    }

    /// <summary>
    /// Sets the provider factory right away while the default app doesn't exist, so the next app configuration reads
    /// it, whether that runs through <c>CrossFirebase.Initialize()</c> or the native <c>App.Configure()</c>. Once the
    /// default app exists, configuring the provider set for it does nothing.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The default app exists, and <paramref name="options"/> selects a provider other than the one set for it, or the
    /// plugin set none.
    /// </exception>
    public void Configure(AppCheckOptions options)
    {
        lock(_syncRoot) {
            if(_isDefaultAppConfigured()) {
                EnsureProviderUnchanged(_installedProvider, options.Provider);
                return;
            }

            // Recorded only after the native call succeeds, so the record always names the factory in effect.
            _setProviderFactory(options.Provider);
            _installedProvider = options.Provider;
        }
    }

    private static void EnsureProviderUnchanged(AppCheckProviderType? installedProvider, AppCheckProviderType requestedProvider)
    {
        if(installedProvider == null) {
            throw new InvalidOperationException(
                $"App Check cannot be configured with the '{requestedProvider}' provider after the default Firebase app is "
                    + "configured, because the native Firebase iOS SDK reads the provider factory only when an app is "
                    + "configured, and the plugin didn't install the provider in effect. Unless the app set a factory "
                    + "through the native binding, the native default DeviceCheck provider is in effect. Call "
                    + "CrossFirebaseAppCheck.Configure() before Firebase is initialized."
            );
        }

        if(installedProvider != requestedProvider) {
            throw new InvalidOperationException(
                $"App Check cannot change from the '{installedProvider}' provider to '{requestedProvider}' after the default "
                    + "Firebase app is configured, because the native Firebase iOS SDK reads the provider factory only when "
                    + "an app is configured. Call CrossFirebaseAppCheck.Configure() before Firebase is initialized."
            );
        }
    }
}

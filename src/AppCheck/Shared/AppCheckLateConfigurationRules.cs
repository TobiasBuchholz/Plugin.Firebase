namespace Plugin.Firebase.AppCheck;

/// <summary>
/// Decides whether App Check can be configured after the default Firebase app is configured, for a native SDK that
/// reads the provider factory only when an app is configured (iOS).
/// </summary>
/// <remarks>
/// Platform-neutral so the rules can be unit tested without a device.
/// </remarks>
internal static class AppCheckLateConfigurationRules
{
    /// <summary>
    /// Returns when <paramref name="requestedProvider"/> is the provider the plugin installed before the default app was
    /// configured, so configuring it again does nothing.
    /// </summary>
    /// <param name="installedProvider">
    /// The provider the plugin installed before the default app was configured, or <c>null</c> when it installed none.
    /// </param>
    /// <param name="requestedProvider">The provider being configured.</param>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="requestedProvider"/> differs from <paramref name="installedProvider"/>, or the plugin installed
    /// no provider, so it doesn't know which one is in effect.
    /// </exception>
    public static void EnsureProviderUnchanged(AppCheckProviderType? installedProvider, AppCheckProviderType requestedProvider)
    {
        if(installedProvider == null) {
            throw new InvalidOperationException(
                $"App Check cannot be configured with the '{requestedProvider}' provider after the default Firebase app is "
                    + "configured, because the native Firebase iOS SDK reads the provider factory only when an app is "
                    + "configured, and the plugin didn't install the provider in effect. Unless the app set a factory "
                    + "through the native binding, the native default DeviceCheck provider is in effect. Call "
                    + "CrossFirebaseAppCheck.Configure() before CrossFirebase.Initialize()."
            );
        }

        if(installedProvider != requestedProvider) {
            throw new InvalidOperationException(
                $"App Check cannot change from the '{installedProvider}' provider to '{requestedProvider}' after the default "
                    + "Firebase app is configured, because the native Firebase iOS SDK reads the provider factory only when "
                    + "an app is configured. Call CrossFirebaseAppCheck.Configure() before CrossFirebase.Initialize()."
            );
        }
    }
}
namespace Plugin.Firebase.AppCheck;

/// <summary>
/// Interface for Firebase AppCheck implementation.
/// </summary>
public interface IFirebaseAppCheck : IDisposable
{
    /// <summary>
    /// Configures Firebase AppCheck with the specified options.
    /// </summary>
    /// <remarks>
    /// Call this before <c>CrossFirebase.Initialize()</c>. On Android, a later call installs the provider factory
    /// right away. On iOS, the native SDK reads the provider factory only when Firebase configures an app, so after
    /// the default app is configured, configuring the provider the plugin installed does nothing. Any other provider
    /// throws, and so does every provider when the plugin installed none, because the native default
    /// (<c>DeviceCheck</c>) or a factory the app set natively may be in effect.
    /// </remarks>
    /// <param name="options">The AppCheck options to apply.</param>
    /// <exception cref="NotSupportedException">
    /// <paramref name="options"/> selects a provider that the current platform does not support.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// On Android, <paramref name="options"/> is <see cref="AppCheckOptions.Disabled"/> while a provider factory is
    /// installed on the default Firebase app. The native SDK cannot remove an installed provider factory.
    /// On iOS, the default Firebase app is already configured, and <paramref name="options"/> selects a provider other
    /// than the one the plugin installed, or the plugin installed none. The native SDK reads the provider factory only
    /// when <c>FirebaseApp.configure()</c> runs.
    /// </exception>
    void Configure(AppCheckOptions options);

    /// <summary>
    /// Fetches an App Check token for the current Firebase app instance.
    /// </summary>
    /// <param name="forceRefresh">If true, bypasses cached tokens when possible.</param>
    /// <returns>The App Check token string.</returns>
    Task<string> GetTokenAsync(bool forceRefresh = false);
}
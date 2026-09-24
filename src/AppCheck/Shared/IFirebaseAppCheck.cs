namespace Plugin.Firebase.AppCheck;

/// <summary>
/// Interface for Firebase AppCheck implementation.
/// </summary>
public interface IFirebaseAppCheck : IDisposable
{
    /// <summary>
    /// Configures Firebase AppCheck with the specified options.
    /// </summary>
    /// <param name="options">The AppCheck options to apply.</param>
    /// <exception cref="NotSupportedException">
    /// On Android, <paramref name="options"/> selects a provider that Android does not support.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// On Android, <paramref name="options"/> is <see cref="AppCheckOptions.Disabled"/> while a provider factory is
    /// installed on the default Firebase app. The native SDK cannot remove an installed provider factory.
    /// </exception>
    void Configure(AppCheckOptions options);

    /// <summary>
    /// Fetches an App Check token for the current Firebase app instance.
    /// </summary>
    /// <param name="forceRefresh">If true, bypasses cached tokens when possible.</param>
    /// <returns>The App Check token string.</returns>
    Task<string> GetTokenAsync(bool forceRefresh = false);
}
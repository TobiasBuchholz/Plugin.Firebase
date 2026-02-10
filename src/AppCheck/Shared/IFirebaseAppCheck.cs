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
    void Configure(AppCheckOptions options);

    /// <summary>
    /// Fetches an App Check token for the current Firebase app instance.
    /// </summary>
    /// <param name="forceRefresh">If true, bypasses cached tokens when possible.</param>
    /// <returns>The App Check token string.</returns>
    Task<string> GetTokenAsync(bool forceRefresh = false);
}
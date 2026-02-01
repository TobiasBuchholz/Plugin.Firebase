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
}
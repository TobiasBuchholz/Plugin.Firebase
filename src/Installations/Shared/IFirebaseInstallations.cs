namespace Plugin.Firebase.Installations;

/// <summary>
/// Interface for Firebase Installations implementation.
/// </summary>
public interface IFirebaseInstallations : IDisposable
{
    /// <summary>
    /// Gets the Firebase installation ID for the current app instance.
    /// </summary>
    /// <returns>The Firebase installation ID.</returns>
    Task<string> GetIdAsync();

    /// <summary>
    /// Gets a Firebase Installations auth token for the current app instance.
    /// </summary>
    /// <param name="forceRefresh">If true, bypasses cached tokens when possible.</param>
    /// <returns>The Firebase Installations auth token string.</returns>
    Task<string> GetTokenAsync(bool forceRefresh = false);

    /// <summary>
    /// Deletes the current Firebase installation data from the client and Firebase backend.
    /// </summary>
    Task DeleteAsync();
}
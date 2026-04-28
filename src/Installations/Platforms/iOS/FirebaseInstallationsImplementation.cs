using Plugin.Firebase.Core;

namespace Plugin.Firebase.Installations;

/// <summary>
/// iOS implementation of the Firebase Installations service.
/// </summary>
public sealed class FirebaseInstallationsImplementation : DisposableBase, IFirebaseInstallations
{
    private readonly global::Firebase.Installations.Installations _installations = global::Firebase.Installations.Installations.DefaultInstance;

    /// <inheritdoc/>
    public async Task<string> GetIdAsync()
    {
        return await _installations.GetInstallationIdAsync();
    }

    /// <inheritdoc/>
    public async Task<string> GetTokenAsync(bool forceRefresh = false)
    {
        var tokenResult = await _installations.GetAuthTokenAsync(forceRefresh);
        return tokenResult.AuthToken;
    }

    /// <inheritdoc/>
    public Task DeleteAsync()
    {
        return _installations.DeleteAsync();
    }
}
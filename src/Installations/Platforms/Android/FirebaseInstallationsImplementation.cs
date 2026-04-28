using Android.Gms.Extensions;
using Firebase.Installations;
using Plugin.Firebase.Core;

namespace Plugin.Firebase.Installations;

public sealed class FirebaseInstallationsImplementation : DisposableBase, IFirebaseInstallations
{
    private readonly FirebaseInstallations _installations = FirebaseInstallations.Instance;

    public async Task<string> GetIdAsync()
    {
        var id = await _installations.GetId().AsAsync<Java.Lang.String>();
        return id.ToString();
    }

    public async Task<string> GetTokenAsync(bool forceRefresh = false)
    {
        var tokenResult = await _installations
            .GetToken(forceRefresh)
            .AsAsync<InstallationTokenResult>();
        return tokenResult.Token;
    }

    public Task DeleteAsync()
    {
        return _installations.Delete().AsAsync();
    }
}
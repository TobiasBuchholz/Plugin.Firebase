using Firebase.Auth;
using Foundation;
using Plugin.Firebase.Auth.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Auth.Platforms.iOS;

public sealed class FirebaseUserWrapper : IFirebaseUser
{
    private readonly User _wrapped;

    public FirebaseUserWrapper(User firebaseUser)
    {
        _wrapped = firebaseUser;
    }

    public override string ToString()
    {
        return $"[{nameof(FirebaseUserWrapper)}: {nameof(Uid)}={Uid}, {nameof(Email)}={Email}]";
    }

    public Task UpdateEmailAsync(string email)
    {
        return _wrapped.UpdateEmailAsync(email);
    }

    public Task UpdatePasswordAsync(string password)
    {
        return _wrapped.UpdatePasswordAsync(password);
    }

    public Task UpdatePhoneNumberAsync(string verificationId, string smsCode)
    {
        return _wrapped.UpdatePhoneNumberCredentialAsync(PhoneAuthProvider.DefaultInstance.GetCredential(verificationId, smsCode));
    }

    public Task UpdateProfileAsync(string displayName = "", string photoUrl = "")
    {
        var request = _wrapped.ProfileChangeRequest();
        if(displayName != "") {
            request.DisplayName = displayName;
        }
        if(photoUrl != "") {
            request.PhotoUrl = photoUrl == null ? null : new NSUrl(photoUrl);
        }
        return request.CommitChangesAsync();
    }

    public Task SendEmailVerificationAsync(ActionCodeSettings actionCodeSettings = null)
    {
        return actionCodeSettings == null
            ? _wrapped.SendEmailVerificationAsync()
            : _wrapped.SendEmailVerificationAsync(actionCodeSettings.ToNative());
    }

    public Task UnlinkAsync(string providerId)
    {
        return _wrapped.UnlinkAsync(providerId);
    }

    public Task DeleteAsync()
    {
        return _wrapped.DeleteAsync();
    }

    public async Task<IAuthTokenResult> GetIdTokenResultAsync(bool forceRefresh = false)
    {
        var result = await _wrapped.GetIdTokenResultAsync(forceRefresh);
        return result.ToAbstract();
    }

    public string Uid => _wrapped.Uid;
    public string DisplayName => _wrapped.DisplayName;
    public string Email => _wrapped.Email;
    public string PhotoUrl => _wrapped.PhotoUrl?.AbsoluteString;
    public string ProviderId => _wrapped.ProviderId;
    public bool IsEmailVerified => _wrapped.IsEmailVerified;
    public bool IsAnonymous => _wrapped.IsAnonymous;
    public IEnumerable<ProviderInfo> ProviderInfos => _wrapped.ProviderData?.Select(x => x.ToAbstract());
    public UserMetadata Metadata => _wrapped.Metadata?.ToAbstract();
}
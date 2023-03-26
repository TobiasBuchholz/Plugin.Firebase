using Android.Gms.Extensions;
using Android.Runtime;
using Firebase.Auth;
using Plugin.Firebase.Auth.Platforms.Android.Extensions;
using Uri = Android.Net.Uri;

namespace Plugin.Firebase.Auth.Platforms.Android;

public sealed class FirebaseUserWrapper : IFirebaseUser
{
    private readonly FirebaseUser _wrapped;

    public FirebaseUserWrapper(FirebaseUser firebaseUser)
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
        return _wrapped.UpdatePhoneNumberAsync(PhoneAuthProvider.GetCredential(verificationId, smsCode));
    }

    public Task UpdateProfileAsync(string displayName = "", string photoUrl = "")
    {
        var builder = new UserProfileChangeRequest.Builder();
        if(displayName != "") {
            builder.SetDisplayName(displayName);
        }
        if(photoUrl != "") {
            builder.SetPhotoUri(photoUrl == null ? null : Uri.Parse(photoUrl));
        }
        return _wrapped.UpdateProfileAsync(builder.Build());
    }

    public Task SendEmailVerificationAsync(ActionCodeSettings actionCodeSettings = null)
    {
        return _wrapped.SendEmailVerificationAsync(actionCodeSettings?.ToNative());
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
        var result = (await _wrapped.GetIdToken(forceRefresh)).JavaCast<GetTokenResult>();
        return result.ToAbstract();
    }

    public string Uid => _wrapped.Uid;
    public string DisplayName => _wrapped.DisplayName;
    public string Email => _wrapped.Email;
    public string PhotoUrl => _wrapped.PhotoUrl?.ToString();
    public string ProviderId => _wrapped.ProviderId;
    public bool IsEmailVerified => _wrapped.IsEmailVerified;
    public bool IsAnonymous => _wrapped.IsAnonymous;
    public IEnumerable<ProviderInfo> ProviderInfos => _wrapped.ProviderData?.Select(x => x.ToAbstract());
    public UserMetadata Metadata => _wrapped.Metadata?.ToAbstract();
}
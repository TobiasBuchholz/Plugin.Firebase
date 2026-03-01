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
        return FirebaseAuthExceptionFactory.Wrap(() => _wrapped.UpdateEmailAsync(email));
    }

    public Task UpdatePasswordAsync(string password)
    {
        return FirebaseAuthExceptionFactory.Wrap(() => _wrapped.UpdatePasswordAsync(password));
    }

    public Task UpdatePhoneNumberAsync(string verificationId, string smsCode)
    {
        return FirebaseAuthExceptionFactory.Wrap(
            () => _wrapped.UpdatePhoneNumberAsync(PhoneAuthProvider.GetCredential(verificationId, smsCode))
        );
    }

    public Task UpdateProfileAsync(string displayName = "", string photoUrl = "")
    {
        var builder = new UserProfileChangeRequest.Builder();
        if(displayName != "") {
            builder.SetDisplayName(displayName);
        }
        if(photoUrl != "") {
            builder.SetPhotoUri(string.IsNullOrEmpty(photoUrl) ? null : Uri.Parse(photoUrl));
        }

        var request = builder.Build();
        return FirebaseAuthExceptionFactory.Wrap(() => _wrapped.UpdateProfileAsync(request));
    }

    public Task SendEmailVerificationAsync(ActionCodeSettings? actionCodeSettings = null)
    {
        var nativeActionCodeSettings = actionCodeSettings?.ToNative();
        return FirebaseAuthExceptionFactory.Wrap(
            () => _wrapped.SendEmailVerificationAsync(nativeActionCodeSettings!)
        );
    }

    public Task UnlinkAsync(string providerId)
    {
        return FirebaseAuthExceptionFactory.Wrap(() => _wrapped.UnlinkAsync(providerId));
    }

    public Task DeleteAsync()
    {
        return FirebaseAuthExceptionFactory.Wrap(() => _wrapped.DeleteAsync());
    }

    public async Task<IAuthTokenResult> GetIdTokenResultAsync(bool forceRefresh = false)
    {
        var result = await FirebaseAuthExceptionFactory.Wrap(
            async () => (await _wrapped.GetIdToken(forceRefresh)).JavaCast<GetTokenResult>()
        );
        return result.ToAbstract();
    }

    public string Uid => _wrapped.Uid;
    public string? DisplayName => _wrapped.DisplayName;
    public string? Email => _wrapped.Email;
    public string? PhotoUrl => _wrapped.PhotoUrl?.ToString();
    public string ProviderId => _wrapped.ProviderId;
    public bool IsEmailVerified => _wrapped.IsEmailVerified;
    public bool IsAnonymous => _wrapped.IsAnonymous;
    public IEnumerable<ProviderInfo>? ProviderInfos =>
        _wrapped.ProviderData?.Select(x => x.ToAbstract());
    public UserMetadata? Metadata => _wrapped.Metadata?.ToAbstract();
}

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
        return WrapAsync(_wrapped.UpdateEmailAsync(email));
    }

    public Task UpdatePasswordAsync(string password)
    {
        return WrapAsync(_wrapped.UpdatePasswordAsync(password));
    }

    public Task UpdatePhoneNumberAsync(string verificationId, string smsCode)
    {
        return WrapAsync(_wrapped.UpdatePhoneNumberAsync(
            PhoneAuthProvider.GetCredential(verificationId, smsCode)));
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
        return WrapAsync(_wrapped.UpdateProfileAsync(builder.Build()));
    }

    public Task SendEmailVerificationAsync(ActionCodeSettings actionCodeSettings = null)
    {
        return WrapAsync(_wrapped.SendEmailVerificationAsync(actionCodeSettings?.ToNative()));
    }

    public Task UnlinkAsync(string providerId)
    {
        return WrapAsync(_wrapped.UnlinkAsync(providerId));
    }

    public Task DeleteAsync()
    {
        return WrapAsync(_wrapped.DeleteAsync());
    }

    public async Task<IAuthTokenResult> GetIdTokenResultAsync(bool forceRefresh = false)
    {
        var result = await WrapAsync<GetTokenResult>(_wrapped.GetIdToken(forceRefresh));
        return result.JavaCast<GetTokenResult>().ToAbstract();
    }

    private static async Task WrapAsync(Task task)
    {
        try {
            await task.ConfigureAwait(false);
        } catch(Exception ex) {
            throw FirebaseAuthExceptionFactory.Create(ex);
        }
    }

    private static async Task<T> WrapAsync<T>(Task<T> task)
    {
        try {
            return await task.ConfigureAwait(false);
        } catch(Exception ex) {
            throw FirebaseAuthExceptionFactory.Create(ex);
        }
    }

    private static async Task WrapAsync(global::Android.Gms.Tasks.Task task)
    {
        try {
            await task.AsAsync().ConfigureAwait(false);
        } catch(Exception ex) {
            throw FirebaseAuthExceptionFactory.Create(ex);
        }
    }

    private static async Task<T> WrapAsync<T>(global::Android.Gms.Tasks.Task task) where T : Java.Lang.Object
    {
        try {
            return await task.AsAsync<T>().ConfigureAwait(false);
        } catch(Exception ex) {
            throw FirebaseAuthExceptionFactory.Create(ex);
        }
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
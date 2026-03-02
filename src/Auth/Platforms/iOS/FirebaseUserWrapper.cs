using Firebase.Auth;
using Plugin.Firebase.Auth.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Auth.Platforms.iOS;

/// <summary>
/// Wraps a native iOS Firebase User for cross-platform access.
/// </summary>
public sealed class FirebaseUserWrapper : IFirebaseUser
{
    private readonly User _wrapped;

    /// <summary>
    /// Initializes a new instance wrapping the specified native Firebase user.
    /// </summary>
    /// <param name="firebaseUser">The native iOS Firebase User to wrap.</param>
    public FirebaseUserWrapper(User firebaseUser)
    {
        _wrapped = firebaseUser;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[{nameof(FirebaseUserWrapper)}: {nameof(Uid)}={Uid}, {nameof(Email)}={Email}]";
    }

    /// <inheritdoc/>
    public Task UpdateEmailAsync(string email)
    {
        return FirebaseAuthExceptionFactory.Wrap(() => _wrapped.UpdateEmailAsync(email));
    }

    /// <inheritdoc/>
    public Task UpdatePasswordAsync(string password)
    {
        return FirebaseAuthExceptionFactory.Wrap(() => _wrapped.UpdatePasswordAsync(password));
    }

    /// <inheritdoc/>
    public Task UpdatePhoneNumberAsync(string verificationId, string smsCode)
    {
        return FirebaseAuthExceptionFactory.Wrap(
            () => _wrapped.UpdatePhoneNumberCredentialAsync(
                PhoneAuthProvider.DefaultInstance.GetCredential(verificationId, smsCode)
            )
        );
    }

    /// <inheritdoc/>
    public Task UpdateProfileAsync(string displayName = "", string photoUrl = "")
    {
        var request = _wrapped.ProfileChangeRequest();
        if(displayName != "") {
            request.DisplayName = displayName;
        }
        if(photoUrl != "") {
            request.PhotoUrl = photoUrl == null ? null : new NSUrl(photoUrl);
        }

        return FirebaseAuthExceptionFactory.Wrap(() => request.CommitChangesAsync());
    }

    /// <inheritdoc/>
    public Task SendEmailVerificationAsync(ActionCodeSettings? actionCodeSettings = null)
    {
        return FirebaseAuthExceptionFactory.Wrap(
            () => actionCodeSettings == null
                ? _wrapped.SendEmailVerificationAsync()
                : _wrapped.SendEmailVerificationAsync(actionCodeSettings.ToNative())
        );
    }

    /// <inheritdoc/>
    public Task UnlinkAsync(string providerId)
    {
        return FirebaseAuthExceptionFactory.Wrap(() => _wrapped.UnlinkAsync(providerId));
    }

    /// <inheritdoc/>
    public Task DeleteAsync()
    {
        return FirebaseAuthExceptionFactory.Wrap(() => _wrapped.DeleteAsync());
    }

    /// <inheritdoc/>
    public async Task<IAuthTokenResult> GetIdTokenResultAsync(bool forceRefresh = false)
    {
        var result = await FirebaseAuthExceptionFactory.Wrap(
            () => _wrapped.GetIdTokenResultAsync(forceRefresh)
        );
        return result.ToAbstract();
    }

    /// <inheritdoc/>
    public string Uid => _wrapped.Uid;

    /// <inheritdoc/>
    public string? DisplayName => _wrapped.DisplayName;

    /// <inheritdoc/>
    public string? Email => _wrapped.Email;

    /// <inheritdoc/>
    public string? PhotoUrl => _wrapped.PhotoUrl?.AbsoluteString;

    /// <inheritdoc/>
    public string ProviderId => _wrapped.ProviderId;

    /// <inheritdoc/>
    public bool IsEmailVerified => _wrapped.IsEmailVerified;

    /// <inheritdoc/>
    public bool IsAnonymous => _wrapped.IsAnonymous;

    /// <inheritdoc/>
    public IEnumerable<ProviderInfo>? ProviderInfos =>
        _wrapped.ProviderData?.Select(x => x.ToAbstract());

    /// <inheritdoc/>
    public UserMetadata? Metadata => _wrapped.Metadata?.ToAbstract();
}
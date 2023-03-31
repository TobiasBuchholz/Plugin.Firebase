using Android.Content;
using Firebase.Auth;
using Microsoft.Maui.ApplicationModel;
using Plugin.Firebase.Core;
using Plugin.Firebase.Auth.Facebook.Platforms.Android;
using Plugin.Firebase.Auth.Platforms.Android.Extensions;
using Plugin.Firebase.Core.Exceptions;
using Activity = Android.App.Activity;
using CrossFirebaseAuthException = Plugin.Firebase.Core.Exceptions.FirebaseAuthException;

namespace Plugin.Firebase.Auth.Facebook;

public sealed class FirebaseAuthFacebookImplementation : DisposableBase, IFirebaseAuthFacebook
{
    public static Task HandleActivityResultAsync(int requestCode, Result resultCode, Intent data)
    {
        _facebookAuth.Value.HandleActivityResult(requestCode, resultCode, data);
        return Task.CompletedTask;
    }

    private readonly FirebaseAuth _firebaseAuth;
    private static Lazy<FacebookAuth> _facebookAuth;

    public FirebaseAuthFacebookImplementation()
    {
        _firebaseAuth = FirebaseAuth.Instance;
        _facebookAuth = new Lazy<FacebookAuth>(() => new FacebookAuth());
    }

    public async Task<IFirebaseUser> SignInWithFacebookAsync()
    {
        try {
            var credential = await _facebookAuth.Value.GetCredentialAsync(Activity);
            return await SignInWithCredentialAsync(credential);
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
    }

    private async Task<IFirebaseUser> SignInWithCredentialAsync(AuthCredential credential)
    {
        var authResult = await _firebaseAuth.SignInWithCredentialAsync(credential);
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    private static CrossFirebaseAuthException GetFirebaseAuthException(Exception ex)
    {
        return ex switch {
            FirebaseAuthEmailException => new CrossFirebaseAuthException(FIRAuthError.InvalidEmail, ex.Message),
            FirebaseAuthInvalidUserException => new CrossFirebaseAuthException(FIRAuthError.UserNotFound, ex.Message),
            FirebaseAuthWeakPasswordException => new CrossFirebaseAuthException(FIRAuthError.WeakPassword, ex.Message),
            FirebaseAuthInvalidCredentialsException { ErrorCode: "ERROR_WRONG_PASSWORD" } => new CrossFirebaseAuthException(FIRAuthError.WrongPassword, ex.Message),
            FirebaseAuthInvalidCredentialsException => new CrossFirebaseAuthException(FIRAuthError.InvalidCredential, ex.Message),
            FirebaseAuthUserCollisionException { ErrorCode: "ERROR_EMAIL_ALREADY_IN_USE" } => new CrossFirebaseAuthException(FIRAuthError.EmailAlreadyInUse, ex.Message),
            FirebaseAuthUserCollisionException { ErrorCode: "ERROR_ACCOUNT_EXISTS_WITH_DIFFERENT_CREDENTIAL" } => new CrossFirebaseAuthException(FIRAuthError.AccountExistsWithDifferentCredential, ex.Message),
            _ => new CrossFirebaseAuthException(FIRAuthError.Undefined, ex.Message)
        };
    }

    public async Task<IFirebaseUser> LinkWithFacebookAsync()
    {
        try {
            var credential = await _facebookAuth.Value.GetCredentialAsync(Activity);
            return await LinkWithCredentialAsync(credential);
        } catch(Exception e) {
            _facebookAuth.Value.SignOut();
            throw GetFirebaseAuthException(e);
        }
    }

    private async Task<IFirebaseUser> LinkWithCredentialAsync(AuthCredential credential)
    {
        var authResult = await _firebaseAuth.CurrentUser.LinkWithCredentialAsync(credential);
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    public Task SignOutAsync()
    {
        return Task.CompletedTask;
    }

    private static Activity Activity =>
        Platform.CurrentActivity ?? throw new NullReferenceException("Platform.CurrentActivity is null");
}
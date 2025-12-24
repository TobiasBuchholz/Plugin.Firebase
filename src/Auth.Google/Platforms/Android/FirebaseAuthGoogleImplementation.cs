using Android.Content;
using AndroidX.Fragment.App;
using Firebase.Auth;
using Microsoft.Maui.ApplicationModel;
using Plugin.Firebase.Auth.Google.Platforms.Android;
using Plugin.Firebase.Auth.Platforms.Android.Extensions;
using Plugin.Firebase.Core;
using Plugin.Firebase.Core.Exceptions;
using Activity = Android.App.Activity;
using CrossFirebaseAuthException = Plugin.Firebase.Core.Exceptions.FirebaseAuthException;

namespace Plugin.Firebase.Auth.Google;

public sealed class FirebaseAuthGoogleImplementation : DisposableBase, IFirebaseAuthGoogle
{
    public static Task HandleActivityResultAsync(int requestCode, Result resultCode, Intent data)
    {
        return _googleAuth.Value.HandleActivityResultAsync(requestCode, resultCode, data);
    }
    
    public static void Initialize(string googleRequestIdToken)
    {
        _googleRequestIdToken = googleRequestIdToken;
    }
    
    private static string _googleRequestIdToken;
    
    private readonly FirebaseAuth _firebaseAuth;
    private static Lazy<GoogleAuth> _googleAuth;

    public FirebaseAuthGoogleImplementation()
    {
        _firebaseAuth = FirebaseAuth.Instance;
        _googleAuth = new Lazy<GoogleAuth>(() => new GoogleAuth(Activity, _googleRequestIdToken));
    }
    
    public async Task<IFirebaseUser> SignInWithGoogleAsync()
    {
        try {
            var credential = await _googleAuth.Value.GetCredentialAsync(FragmentActivity);
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
    
    public async Task<IFirebaseUser> LinkWithGoogleAsync()
    {
        try {
            var credential = await _googleAuth.Value.GetCredentialAsync(FragmentActivity);
            return await LinkWithCredentialAsync(credential);
        } catch(Exception e) {
            await _googleAuth.Value.SignOutAsync();
            throw GetFirebaseAuthException(e);
        }
    }
    
    private async Task<IFirebaseUser> LinkWithCredentialAsync(AuthCredential credential)
    {
        var authResult = await _firebaseAuth.CurrentUser.LinkWithCredentialAsync(credential);
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }
    
    public async Task SignOutAsync()
    {
        try {
            await _googleAuth.Value.SignOutAsync();
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
    }
    
    private static Activity Activity =>
        Platform.CurrentActivity ?? throw new NullReferenceException("Platform.CurrentActivity is null");
    
    private static FragmentActivity FragmentActivity =>
        Activity as FragmentActivity ?? throw new NullReferenceException($"Current Activity is either null or not of type {nameof(FragmentActivity)}, which is mandatory for sign in with Google");
}
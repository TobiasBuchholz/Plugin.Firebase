using Android.Gms.Extensions;
using Firebase.Auth;
using Plugin.Firebase.Auth.Platforms.Android.Email;
using Plugin.Firebase.Auth.Platforms.Android.PhoneNumber;
using Plugin.Firebase.Auth.Platforms.Android.Extensions;
using Plugin.Firebase.Core;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Core.Platforms.Android;
using CrossActionCodeSettings = Plugin.Firebase.Auth.ActionCodeSettings;
using CrossFirebaseAuthException = Plugin.Firebase.Core.Exceptions.FirebaseAuthException;

namespace Plugin.Firebase.Auth;

public sealed class FirebaseAuthImplementation : DisposableBase, IFirebaseAuth
{
    private readonly FirebaseAuth _firebaseAuth;
    private readonly EmailAuth _emailAuth;
    private readonly PhoneNumberAuth _phoneNumberAuth;

    public FirebaseAuthImplementation()
    {
        _firebaseAuth = FirebaseAuth.Instance;
        _emailAuth = new EmailAuth();
        _phoneNumberAuth = new PhoneNumberAuth();

        // apply the default app language for sending emails 
        _firebaseAuth.UseAppLanguage();
    }

    public async Task VerifyPhoneNumberAsync(string phoneNumber)
    {
        try {
            var activityLocator = CrossFirebase.ActivityLocator;
            if(activityLocator is null) {
                throw new InvalidOperationException("ActivityLocator is null.");
            }
            var activity = activityLocator();
            if(activity is null) {
                throw new InvalidOperationException("Activity is null.");
            }

            await _phoneNumberAuth.VerifyPhoneNumberAsync(activity, phoneNumber);
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
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

    public async Task<IFirebaseUser> SignInWithCustomTokenAsync(string token)
    {
        try {
            var authResult = await _firebaseAuth.SignInWithCustomTokenAsync(token);
            return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
    }

    public async Task<IFirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode)
    {
        try {
            var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
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

    public async Task<IFirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password, bool createsUserAutomatically = true)
    {
        try {
            var credential = await _emailAuth.GetCredentialAsync(email, password);
            return await SignInWithCredentialAsync(credential);
        } catch(Exception e) {
            if(e is FirebaseAuthInvalidUserException && createsUserAutomatically) {
                return await CreateUserAsync(email, password);
            }
            throw GetFirebaseAuthException(e);
        }
    }

    public async Task<IFirebaseUser> CreateUserAsync(string email, string password)
    {
        try {
            return await _emailAuth.CreateUserAsync(email, password);
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
    }

    public async Task<IFirebaseUser> SignInWithEmailLinkAsync(string email, string link)
    {
        try {
            await _firebaseAuth.SignInWithEmailLink(email, link);
            return _firebaseAuth.CurrentUser.ToAbstract();
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
    }

    public async Task<IFirebaseUser> SignInAnonymouslyAsync()
    {
        try {
            var authResult = await _firebaseAuth.SignInAnonymouslyAsync();
            return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
    }

    public async Task<IFirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(string verificationCode)
    {
        try {
            var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
            return await LinkWithCredentialAsync(credential);
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
    }

    private async Task<IFirebaseUser> LinkWithCredentialAsync(AuthCredential credential)
    {
        var currentUser = _firebaseAuth.CurrentUser;
        if(currentUser is null) {
            throw new FirebaseException("CurrentUser is null. You need to be logged in to use this feature.");
        }
        var authResult = await currentUser.LinkWithCredentialAsync(credential);
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    public async Task<IFirebaseUser> LinkWithEmailAndPasswordAsync(string email, string password)
    {
        try {
            var credential = await _emailAuth.GetCredentialAsync(email, password);
            return await LinkWithCredentialAsync(credential);
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
    }

    public async Task SendSignInLink(string toEmail, CrossActionCodeSettings actionCodeSettings)
    {
        try {
            await _firebaseAuth.SendSignInLinkToEmail(toEmail, actionCodeSettings.ToNative());
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
    }

    public Task SignOutAsync()
    {
        try {
            _firebaseAuth.SignOut();
            return Task.CompletedTask;
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
    }

    public bool IsSignInWithEmailLink(string link)
    {
        try {
            return _firebaseAuth.IsSignInWithEmailLink(link);
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
    }

    public Task SendPasswordResetEmailAsync()
    {
        var currentUser = _firebaseAuth.CurrentUser;
        if(currentUser is null) {
            throw new FirebaseException("CurrentUser is null. You need to be logged in to use this feature.");
        }

        var email = currentUser.Email;
        if(email is null) {
            throw new FirebaseException("CurrentUser.Email is null.");
        }

        return _firebaseAuth.SendPasswordResetEmailAsync(email);
    }

    public Task SendPasswordResetEmailAsync(string email)
    {
        return _firebaseAuth.SendPasswordResetEmailAsync(email);
    }

    public string? LanguageCode {
        set => _firebaseAuth.LanguageCode = value;
    }

    public void UseAppLanguage()
    {
        _firebaseAuth.UseAppLanguage();
    }

    public void UseEmulator(string host, int port)
    {
        _firebaseAuth.UseEmulator(host, port);
    }

    public IDisposable AddAuthStateListener(Action<IFirebaseAuth> listener)
    {
        var authStateListener = new AuthStateListener(_ => listener.Invoke(this));
        _firebaseAuth.AddAuthStateListener(authStateListener);
        return new DisposableWithAction(() => _firebaseAuth.RemoveAuthStateListener(authStateListener));
    }

    public IFirebaseUser CurrentUser => _firebaseAuth.CurrentUser?.ToAbstract();

    private class AuthStateListener : Java.Lang.Object, FirebaseAuth.IAuthStateListener
    {
        private readonly Action<FirebaseAuth> _onAuthStateChanged;

        public AuthStateListener(Action<FirebaseAuth> onAuthStateChanged)
        {
            _onAuthStateChanged = onAuthStateChanged;
        }

        public void OnAuthStateChanged(FirebaseAuth auth)
        {
            _onAuthStateChanged.Invoke(auth);
        }
    }
}
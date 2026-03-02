using Android.Gms.Extensions;
using Firebase.Auth;
using Plugin.Firebase.Auth.Platforms.Android.Email;
using Plugin.Firebase.Auth.Platforms.Android.Extensions;
using Plugin.Firebase.Auth.Platforms.Android.PhoneNumber;
using Plugin.Firebase.Core;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Core.Platforms.Android;
using CrossActionCodeSettings = Plugin.Firebase.Auth.ActionCodeSettings;

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
        var activityLocator = CrossFirebase.ActivityLocator;
        if(activityLocator is null) {
            throw new InvalidOperationException("ActivityLocator is null.");
        }
        var activity = activityLocator();
        if(activity is null) {
            throw new InvalidOperationException("Activity is null.");
        }

        await FirebaseAuthExceptionFactory.Wrap(() => _phoneNumberAuth.VerifyPhoneNumberAsync(activity, phoneNumber));
    }

    public async Task<IFirebaseUser> SignInWithCustomTokenAsync(string token)
    {
        var authResult = await FirebaseAuthExceptionFactory.Wrap(
            () => _firebaseAuth.SignInWithCustomTokenAsync(token)
        );
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    public async Task<IFirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode)
    {
        var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
        return await SignInWithCredentialAsync(credential);
    }

    private async Task<IFirebaseUser> SignInWithCredentialAsync(AuthCredential credential)
    {
        var authResult = await FirebaseAuthExceptionFactory.Wrap(
            () => _firebaseAuth.SignInWithCredentialAsync(credential)
        );
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    public async Task<IFirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password, bool createsUserAutomatically = true)
    {
        try {
            var credential = await _emailAuth.GetCredentialAsync(email, password);
            return await SignInWithCredentialAsync(credential);
        } catch(CrossPlatformFirebaseAuthException e)
            when (createsUserAutomatically && ShouldAttemptCreateUser(e)) {
            // Firebase Android SDK with Email Enumeration Protection enabled (default)
            // may return FirebaseAuthInvalidCredentialsException instead of
            // FirebaseAuthInvalidUserException when the user does not exist.
            // We attempt user creation for both; if the user actually exists
            // (wrong password scenario), creation fails and we re-throw the original error.
            try {
                return await CreateUserAsync(email, password);
            } catch(CrossPlatformFirebaseAuthException) {
                throw e;
            }
        }
    }

    public Task<IFirebaseUser> CreateUserAsync(string email, string password)
    {
        return FirebaseAuthExceptionFactory.Wrap(() => _emailAuth.CreateUserAsync(email, password));
    }

    public async Task<IFirebaseUser> SignInWithEmailLinkAsync(string email, string link)
    {
        await FirebaseAuthExceptionFactory.Wrap(async () => {
            await _firebaseAuth.SignInWithEmailLink(email, link);
        });
        return _firebaseAuth.CurrentUser.ToAbstract();
    }

    public async Task<IFirebaseUser> SignInAnonymouslyAsync()
    {
        var authResult = await FirebaseAuthExceptionFactory.Wrap(
            () => _firebaseAuth.SignInAnonymouslyAsync()
        );
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    public async Task<IFirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(string verificationCode)
    {
        var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
        return await LinkWithCredentialAsync(credential);
    }

    private async Task<IFirebaseUser> LinkWithCredentialAsync(AuthCredential credential)
    {
        var currentUser = _firebaseAuth.CurrentUser;
        if(currentUser is null) {
            throw new FirebaseException("CurrentUser is null. You need to be logged in to use this feature.");
        }

        var authResult = await FirebaseAuthExceptionFactory.Wrap(
            () => currentUser.LinkWithCredentialAsync(credential)
        );
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    public async Task<IFirebaseUser> LinkWithEmailAndPasswordAsync(string email, string password)
    {
        var credential = await _emailAuth.GetCredentialAsync(email, password);
        return await LinkWithCredentialAsync(credential);
    }

    public async Task SendSignInLink(string toEmail, CrossActionCodeSettings actionCodeSettings)
    {
        var nativeActionCodeSettings = actionCodeSettings.ToNative();
        if(nativeActionCodeSettings is null) {
            throw new InvalidOperationException("ActionCodeSettings.ToNative() returned null.");
        }

        await FirebaseAuthExceptionFactory.Wrap(async () => {
            await _firebaseAuth.SendSignInLinkToEmail(toEmail, nativeActionCodeSettings);
        });
    }

    public Task SignOutAsync()
    {
        try {
            _firebaseAuth.SignOut();
            return Task.CompletedTask;
        } catch(Exception e) when (FirebaseAuthExceptionFactory.IsNativeAuthException(e)) {
            throw FirebaseAuthExceptionFactory.Create(e);
        }
    }

    public bool IsSignInWithEmailLink(string link)
    {
        try {
            return _firebaseAuth.IsSignInWithEmailLink(link);
        } catch(Exception e) when (FirebaseAuthExceptionFactory.IsNativeAuthException(e)) {
            throw FirebaseAuthExceptionFactory.Create(e);
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

        return FirebaseAuthExceptionFactory.Wrap(() => _firebaseAuth.SendPasswordResetEmailAsync(email));
    }

    public Task SendPasswordResetEmailAsync(string email)
    {
        return FirebaseAuthExceptionFactory.Wrap(() => _firebaseAuth.SendPasswordResetEmailAsync(email));
    }

    public async Task ReloadCurrentUserAsync()
    {
        var currentUser = _firebaseAuth.CurrentUser;
        if(currentUser is null) {
            throw new FirebaseException(
                "CurrentUser is null. You need to be logged in to use this feature."
            );
        }

        await FirebaseAuthExceptionFactory.Wrap(() => currentUser.ReloadAsync());
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

    private static bool ShouldAttemptCreateUser(CrossPlatformFirebaseAuthException exception)
    {
        return exception.NativeExceptionTypeName is
            FirebaseAuthAndroidExceptionTypeNames.InvalidUserException or
            FirebaseAuthAndroidExceptionTypeNames.InvalidCredentialsException;
    }

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

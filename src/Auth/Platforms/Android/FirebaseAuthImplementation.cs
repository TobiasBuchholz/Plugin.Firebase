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

        try {
            await _phoneNumberAuth.VerifyPhoneNumberAsync(activity, phoneNumber);
        } catch(Exception e) when (FirebaseAuthExceptionFactory.IsNativeAuthException(e)) {
            throw FirebaseAuthExceptionFactory.Create(e);
        }
    }

    public async Task<IFirebaseUser> SignInWithCustomTokenAsync(string token)
    {
        try {
            var authResult = await _firebaseAuth.SignInWithCustomTokenAsync(token);
            return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
        } catch(Exception e) when (FirebaseAuthExceptionFactory.IsNativeAuthException(e)) {
            throw FirebaseAuthExceptionFactory.Create(e);
        }
    }

    public async Task<IFirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode)
    {
        try {
            var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
            return await SignInWithCredentialAsync(credential);
        } catch(Exception e) when (FirebaseAuthExceptionFactory.IsNativeAuthException(e)) {
            throw FirebaseAuthExceptionFactory.Create(e);
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
        } catch(Exception e) when (FirebaseAuthExceptionFactory.IsNativeAuthException(e)) {
            // Firebase Android SDK with Email Enumeration Protection enabled (default)
            // may return FirebaseAuthInvalidCredentialsException instead of
            // FirebaseAuthInvalidUserException when the user does not exist.
            // We attempt user creation for both; if the user actually exists
            // (wrong password scenario), creation fails and we re-throw the original error.
            if(createsUserAutomatically &&
                (e is FirebaseAuthInvalidUserException ||
                 e is FirebaseAuthInvalidCredentialsException)) {
                try {
                    return await CreateUserAsync(email, password);
                } catch(CrossPlatformFirebaseAuthException) {
                    throw FirebaseAuthExceptionFactory.Create(e);
                }
            }
            throw FirebaseAuthExceptionFactory.Create(e);
        }
    }

    public async Task<IFirebaseUser> CreateUserAsync(string email, string password)
    {
        try {
            return await _emailAuth.CreateUserAsync(email, password);
        } catch(Exception e) when (FirebaseAuthExceptionFactory.IsNativeAuthException(e)) {
            throw FirebaseAuthExceptionFactory.Create(e);
        }
    }

    public async Task<IFirebaseUser> SignInWithEmailLinkAsync(string email, string link)
    {
        try {
            await _firebaseAuth.SignInWithEmailLink(email, link);
            return _firebaseAuth.CurrentUser.ToAbstract();
        } catch(Exception e) when (FirebaseAuthExceptionFactory.IsNativeAuthException(e)) {
            throw FirebaseAuthExceptionFactory.Create(e);
        }
    }

    public async Task<IFirebaseUser> SignInAnonymouslyAsync()
    {
        try {
            var authResult = await _firebaseAuth.SignInAnonymouslyAsync();
            return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
        } catch(Exception e) when (FirebaseAuthExceptionFactory.IsNativeAuthException(e)) {
            throw FirebaseAuthExceptionFactory.Create(e);
        }
    }

    public async Task<IFirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(string verificationCode)
    {
        try {
            var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
            return await LinkWithCredentialAsync(credential);
        } catch(Exception e) when (FirebaseAuthExceptionFactory.IsNativeAuthException(e)) {
            throw FirebaseAuthExceptionFactory.Create(e);
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
        } catch(Exception e) when (FirebaseAuthExceptionFactory.IsNativeAuthException(e)) {
            throw FirebaseAuthExceptionFactory.Create(e);
        }
    }

    public async Task SendSignInLink(string toEmail, CrossActionCodeSettings actionCodeSettings)
    {
        var nativeActionCodeSettings = actionCodeSettings.ToNative();
        if(nativeActionCodeSettings is null) {
            throw new InvalidOperationException("ActionCodeSettings.ToNative() returned null.");
        }

        try {
            await _firebaseAuth.SendSignInLinkToEmail(toEmail, nativeActionCodeSettings);
        } catch(Exception e) when (FirebaseAuthExceptionFactory.IsNativeAuthException(e)) {
            throw FirebaseAuthExceptionFactory.Create(e);
        }
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

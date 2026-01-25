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
        var activityLocator = CrossFirebase.ActivityLocator;
        if(activityLocator is null) {
            throw new InvalidOperationException("ActivityLocator is null.");
        }
        var activity = activityLocator();
        if(activity is null) {
            throw new InvalidOperationException("Activity is null.");
        }

        await WrapAsync(_phoneNumberAuth.VerifyPhoneNumberAsync(activity, phoneNumber));
    }

    private static CrossFirebaseAuthException GetFirebaseAuthException(Exception ex) =>
        Plugin.Firebase.Auth.Platforms.Android.FirebaseAuthExceptionFactory.Create(ex);

    public async Task<IFirebaseUser> SignInWithCustomTokenAsync(string token)
    {
        var authResult = await WrapAsync(_firebaseAuth.SignInWithCustomTokenAsync(token));
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    public async Task<IFirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode)
    {
        var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
        return await SignInWithCredentialAsync(credential);
    }

    private async Task<IFirebaseUser> SignInWithCredentialAsync(AuthCredential credential)
    {
        var authResult = await WrapAsync(_firebaseAuth.SignInWithCredentialAsync(credential));
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    public async Task<IFirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password, bool createsUserAutomatically = true)
    {
        var credential = await _emailAuth.GetCredentialAsync(email, password);
        try {
            return await SignInWithCredentialAsync(credential);
        } catch(CrossFirebaseAuthException e) when(
            e.Reason == FIRAuthError.UserNotFound && createsUserAutomatically) {
            return await CreateUserAsync(email, password);
        }
    }

    public async Task<IFirebaseUser> CreateUserAsync(string email, string password)
    {
        return await WrapAsync(_emailAuth.CreateUserAsync(email, password));
    }

    public async Task<IFirebaseUser> SignInWithEmailLinkAsync(string email, string link)
    {
        await WrapAsync(_firebaseAuth.SignInWithEmailLink(email, link));
        return _firebaseAuth.CurrentUser.ToAbstract();
    }

    public async Task<IFirebaseUser> SignInAnonymouslyAsync()
    {
        var authResult = await WrapAsync(_firebaseAuth.SignInAnonymouslyAsync());
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
        var authResult = await WrapAsync(currentUser.LinkWithCredentialAsync(credential));
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    public async Task<IFirebaseUser> LinkWithEmailAndPasswordAsync(string email, string password)
    {
        var credential = await _emailAuth.GetCredentialAsync(email, password);
        return await LinkWithCredentialAsync(credential);
    }

    public async Task SendSignInLink(string toEmail, CrossActionCodeSettings actionCodeSettings)
    {
        await WrapAsync(_firebaseAuth.SendSignInLinkToEmail(toEmail, actionCodeSettings.ToNative()));
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

    public async Task SendPasswordResetEmailAsync()
    {
        var currentUser = _firebaseAuth.CurrentUser;
        if(currentUser is null) {
            throw new FirebaseException("CurrentUser is null. You need to be logged in to use this feature.");
        }

        var email = currentUser.Email;
        if(email is null) {
            throw new FirebaseException("CurrentUser.Email is null.");
        }

        await WrapAsync(_firebaseAuth.SendPasswordResetEmailAsync(email));
    }

    public async Task SendPasswordResetEmailAsync(string email)
    {
        await WrapAsync(_firebaseAuth.SendPasswordResetEmailAsync(email));
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

    private static async Task WrapAsync(Task task)
    {
        try {
            await task.ConfigureAwait(false);
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
    }

    private static async Task<T> WrapAsync<T>(Task<T> task)
    {
        try {
            return await task.ConfigureAwait(false);
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
    }

    private static async Task WrapAsync(global::Android.Gms.Tasks.Task task)
    {
        try {
            await task.AsAsync().ConfigureAwait(false);
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
    }

    private static async Task<T> WrapAsync<T>(global::Android.Gms.Tasks.Task task) where T : Java.Lang.Object
    {
        try {
            return await task.AsAsync<T>().ConfigureAwait(false);
        } catch(Exception e) {
            throw GetFirebaseAuthException(e);
        }
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
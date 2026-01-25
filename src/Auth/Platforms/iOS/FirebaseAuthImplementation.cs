using Firebase.Auth;
using Plugin.Firebase.Auth.Platforms.iOS.Email;
using Plugin.Firebase.Auth.Platforms.iOS.Extensions;
using Plugin.Firebase.Auth.Platforms.iOS.PhoneNumber;
using Plugin.Firebase.Core;
using Plugin.Firebase.Core.Exceptions;
using FirebaseAuth = Firebase.Auth.Auth;
using Task = System.Threading.Tasks.Task;
using CrossActionCodeSettings = Plugin.Firebase.Auth.ActionCodeSettings;

namespace Plugin.Firebase.Auth;

public sealed class FirebaseAuthImplementation : DisposableBase, IFirebaseAuth
{
    private readonly FirebaseAuth _firebaseAuth;
    private readonly EmailAuth _emailAuth;
    private readonly PhoneNumberAuth _phoneNumberAuth;

    public FirebaseAuthImplementation()
    {
        _firebaseAuth = FirebaseAuth.DefaultInstance;
        if(_firebaseAuth is null) {
            throw new FirebaseException("FirebaseAuth.DefaultInstance is null");
        }
        _emailAuth = new EmailAuth();
        _phoneNumberAuth = new PhoneNumberAuth();

        // apply the default app language for sending emails
        _firebaseAuth.UseAppLanguage();
    }

    public async Task VerifyPhoneNumberAsync(string phoneNumber)
    {
        var viewController = GetViewController();
        await WrapAsync(_phoneNumberAuth.VerifyPhoneNumberAsync(viewController, phoneNumber));
    }

    private static FirebaseAuthException GetFirebaseAuthException(NSErrorException ex) =>
        Plugin.Firebase.Auth.Platforms.iOS.FirebaseAuthExceptionFactory.Create(ex);

    public async Task<IFirebaseUser> SignInWithCustomTokenAsync(string token)
    {
        var user = await WrapAsync(_firebaseAuth.SignInWithCustomTokenAsync(token));
        return user.User.ToAbstract();
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
        } catch(FirebaseAuthException e) when(
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
        var authResult = await WrapAsync(_firebaseAuth.SignInWithLinkAsync(email, link));
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
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
            throw new InvalidOperationException("User must be signed in to link with credential.");
        }

        var authResult = await WrapAsync(currentUser.LinkAsync(credential));
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    public async Task<IFirebaseUser> LinkWithEmailAndPasswordAsync(string email, string password)
    {
        var credential = await _emailAuth.GetCredentialAsync(email, password);
        return await LinkWithCredentialAsync(credential);
    }

    public async Task SendSignInLink(string toEmail, CrossActionCodeSettings actionCodeSettings)
    {
        await WrapAsync(_firebaseAuth.SendSignInLinkAsync(toEmail, actionCodeSettings.ToNative()));
    }

    public Task SignOutAsync()
    {
        _firebaseAuth.SignOut(out var error);

        return error is null
            ? Task.CompletedTask
            : throw GetFirebaseAuthException(new NSErrorException(error));
    }

    public bool IsSignInWithEmailLink(string link)
    {
        try {
            return _firebaseAuth.IsSignIn(link);
        } catch(NSErrorException e) {
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

        await WrapAsync(_firebaseAuth.SendPasswordResetAsync(email));
    }

    public async Task SendPasswordResetEmailAsync(string email)
    {
        await WrapAsync(_firebaseAuth.SendPasswordResetAsync(email));
    }

    public void UseEmulator(string host, int port)
    {
        _firebaseAuth.UseEmulatorWithHost(host, port);
    }

    public IDisposable AddAuthStateListener(Action<IFirebaseAuth> listener)
    {
        var handle = _firebaseAuth.AddAuthStateDidChangeListener((_, _) => listener.Invoke(this));
        return new DisposableWithAction(() => _firebaseAuth.RemoveAuthStateDidChangeListener(handle));
    }

    private static UIViewController GetViewController()
    {
        var windowScene = UIApplication.SharedApplication.ConnectedScenes.ToArray()
                .FirstOrDefault(static x => x.ActivationState == UISceneActivationState.ForegroundActive)
            as UIWindowScene;
        var window = windowScene?.Windows.FirstOrDefault(static x => x.IsKeyWindow);
        var rootViewController = window?.RootViewController;

        if(rootViewController is null) {
            throw new InvalidOperationException("RootViewController is null");
        }

        return rootViewController.PresentedViewController ?? rootViewController;
    }

    private static async Task WrapAsync(Task task)
    {
        try {
            await task.ConfigureAwait(false);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    private static async Task<T> WrapAsync<T>(Task<T> task)
    {
        try {
            return await task.ConfigureAwait(false);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    public IFirebaseUser CurrentUser => _firebaseAuth.CurrentUser?.ToAbstract();
}
using Firebase.Auth;
using Plugin.Firebase.Auth.Platforms.iOS.Email;
using Plugin.Firebase.Auth.Platforms.iOS.Extensions;
using Plugin.Firebase.Auth.Platforms.iOS.PhoneNumber;
using Plugin.Firebase.Core;
using Plugin.Firebase.Core.Exceptions;
using CrossActionCodeSettings = Plugin.Firebase.Auth.ActionCodeSettings;
using FirebaseAuth = Firebase.Auth.Auth;
using Task = System.Threading.Tasks.Task;

namespace Plugin.Firebase.Auth;

/// <summary>
/// iOS implementation of Firebase Authentication that wraps the native Firebase Auth SDK.
/// </summary>
public sealed class FirebaseAuthImplementation : DisposableBase, IFirebaseAuth
{
    private readonly FirebaseAuth _firebaseAuth;
    private readonly EmailAuth _emailAuth;
    private readonly PhoneNumberAuth _phoneNumberAuth;

    /// <summary>
    /// Initializes a new instance of the Firebase Auth implementation for iOS.
    /// </summary>
    /// <exception cref="FirebaseException">Thrown when FirebaseAuth.DefaultInstance is null.</exception>
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

    /// <inheritdoc/>
    public async Task VerifyPhoneNumberAsync(string phoneNumber)
    {
        try {
            var viewController = GetViewController();
            await _phoneNumberAuth.VerifyPhoneNumberAsync(viewController, phoneNumber);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    private static FirebaseAuthException GetFirebaseAuthException(NSErrorException ex)
    {
        AuthErrorCode errorCode;
        if(IntPtr.Size == 8) { // 64 bits devices
            errorCode = (AuthErrorCode) ex.Error.Code;
        } else { // 32 bits devices
            errorCode = (AuthErrorCode) (int) ex.Error.Code;
        }

        Enum.TryParse(errorCode.ToString(), out FIRAuthError authError);
        return new FirebaseAuthException(authError, ex.Error.LocalizedDescription);
    }

    /// <inheritdoc/>
    public async Task<IFirebaseUser> SignInWithCustomTokenAsync(string token)
    {
        try {
            var user = await _firebaseAuth.SignInWithCustomTokenAsync(token);
            return user.User.ToAbstract();
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    /// <inheritdoc/>
    public async Task<IFirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(
        string verificationCode
    )
    {
        try {
            var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
            return await SignInWithCredentialAsync(credential);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    private async Task<IFirebaseUser> SignInWithCredentialAsync(AuthCredential credential)
    {
        var authResult = await _firebaseAuth.SignInWithCredentialAsync(credential);
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    /// <inheritdoc/>
    public async Task<IFirebaseUser> SignInWithEmailAndPasswordAsync(
        string email,
        string password,
        bool createsUserAutomatically = true
    )
    {
        try {
            var credential = await _emailAuth.GetCredentialAsync(email, password);
            return await SignInWithCredentialAsync(credential);
        } catch(NSErrorException e) {
            if(e.Code == (long) AuthErrorCode.UserNotFound && createsUserAutomatically) {
                return await CreateUserAsync(email, password);
            }
            throw GetFirebaseAuthException(e);
        }
    }

    /// <inheritdoc/>
    public async Task<IFirebaseUser> CreateUserAsync(string email, string password)
    {
        try {
            return await _emailAuth.CreateUserAsync(email, password);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    /// <inheritdoc/>
    public async Task<IFirebaseUser> SignInWithEmailLinkAsync(string email, string link)
    {
        try {
            var authResult = await _firebaseAuth.SignInWithLinkAsync(email, link);
            return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    /// <inheritdoc/>
    public async Task<IFirebaseUser> SignInAnonymouslyAsync()
    {
        try {
            var authResult = await _firebaseAuth.SignInAnonymouslyAsync();
            return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    /// <inheritdoc/>
    public async Task<IFirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(
        string verificationCode
    )
    {
        try {
            var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
            return await LinkWithCredentialAsync(credential);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    private async Task<IFirebaseUser> LinkWithCredentialAsync(AuthCredential credential)
    {
        var currentUser = _firebaseAuth.CurrentUser;
        if(currentUser is null) {
            throw new InvalidOperationException("User must be signed in to link with credential.");
        }

        var authResult = await currentUser.LinkAsync(credential);
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }
    
    /// <inheritdoc/>
    public async Task<IFirebaseUser> LinkWithEmailAndPasswordAsync(string email, string password)
    {
        try {
            var credential = await _emailAuth.GetCredentialAsync(email, password);
            return await LinkWithCredentialAsync(credential);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    /// <inheritdoc/>
    public async Task SendSignInLink(string toEmail, CrossActionCodeSettings actionCodeSettings)
    {
        try {
            await _firebaseAuth.SendSignInLinkAsync(toEmail, actionCodeSettings.ToNative());
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    /// <inheritdoc/>
    public Task SignOutAsync()
    {
        _firebaseAuth.SignOut(out var error);

        return error is null
            ? Task.CompletedTask
            : throw new FirebaseException("Errored signing out", new NSErrorException(error));
    }

    /// <inheritdoc/>
    public bool IsSignInWithEmailLink(string link)
    {
        try {
            return _firebaseAuth.IsSignIn(link);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    /// <inheritdoc/>
    public Task SendPasswordResetEmailAsync()
    {
        var currentUser = _firebaseAuth.CurrentUser;
        if(currentUser is null) {
            throw new FirebaseException(
                "CurrentUser is null. You need to be logged in to use this feature."
            );
        }

        var email = currentUser.Email;
        return email is null
            ? throw new FirebaseException("CurrentUser.Email is null.")
            : _firebaseAuth.SendPasswordResetAsync(email);
    }

    /// <inheritdoc/>
    public Task SendPasswordResetEmailAsync(string email)
    {
        return _firebaseAuth.SendPasswordResetAsync(email);
    }

    /// <inheritdoc/>
    public async Task ReloadCurrentUserAsync()
    {
        var currentUser = _firebaseAuth.CurrentUser;
        if(currentUser is null) {
            throw new FirebaseException(
                "CurrentUser is null. You need to be logged in to use this feature."
            );
        }

        await currentUser.ReloadAsync();
    }

    /// <inheritdoc/>
    public void UseEmulator(string host, int port)
    {
        _firebaseAuth.UseEmulatorWithHost(host, port);
    }

    /// <inheritdoc/>
    public IDisposable AddAuthStateListener(Action<IFirebaseAuth> listener)
    {
        var handle = _firebaseAuth.AddAuthStateDidChangeListener((_, _) => listener.Invoke(this));
        return new DisposableWithAction(() =>
            _firebaseAuth.RemoveAuthStateDidChangeListener(handle)
        );
    }

    private static UIViewController GetViewController()
    {
        var windowScene =
            UIApplication
                .SharedApplication.ConnectedScenes.ToArray()
                .FirstOrDefault(static x =>
                    x.ActivationState == UISceneActivationState.ForegroundActive
                ) as UIWindowScene;
        var window = windowScene?.Windows.FirstOrDefault(static x => x.IsKeyWindow);
        var rootViewController = window?.RootViewController;

        if(rootViewController is null) {
            throw new InvalidOperationException("RootViewController is null");
        }

        return rootViewController.PresentedViewController ?? rootViewController;
    }

    /// <inheritdoc/>
    public IFirebaseUser CurrentUser => _firebaseAuth.CurrentUser?.ToAbstract();
}
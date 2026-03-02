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
        var firebaseAuth = FirebaseAuth.DefaultInstance;
        if(firebaseAuth is null) {
            throw new FirebaseException("FirebaseAuth.DefaultInstance is null");
        }
        _firebaseAuth = firebaseAuth;
        _emailAuth = new EmailAuth();
        _phoneNumberAuth = new PhoneNumberAuth();

        // apply the default app language for sending emails
        _firebaseAuth.UseAppLanguage();
    }

    /// <inheritdoc/>
    public async Task VerifyPhoneNumberAsync(string phoneNumber)
    {
        var viewController = GetViewController();
        await FirebaseAuthExceptionFactory.Wrap(
            () => _phoneNumberAuth.VerifyPhoneNumberAsync(viewController, phoneNumber)
        );
    }

    /// <inheritdoc/>
    public async Task<IFirebaseUser> SignInWithCustomTokenAsync(string token)
    {
        var user = await FirebaseAuthExceptionFactory.Wrap(
            () => _firebaseAuth.SignInWithCustomTokenAsync(token)
        );
        return user.User.ToAbstract();
    }

    /// <inheritdoc/>
    public async Task<IFirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(
        string verificationCode
    )
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
        } catch(CrossPlatformFirebaseAuthException e)
            when(createsUserAutomatically && ShouldAttemptCreateUser(e)) {
            // Firebase iOS SDK 10.18+ returns InvalidCredential (INVALID_LOGIN_CREDENTIALS)
            // instead of UserNotFound when Email Enumeration Protection is enabled (default).
            // We attempt user creation for both error codes; if the user actually exists
            // (wrong password scenario), creation fails and we re-throw the original error.
            try {
                return await CreateUserAsync(email, password);
            } catch(CrossPlatformFirebaseAuthException) {
                throw e;
            }
        }
    }

    /// <inheritdoc/>
    public Task<IFirebaseUser> CreateUserAsync(string email, string password)
    {
        return FirebaseAuthExceptionFactory.Wrap(() => _emailAuth.CreateUserAsync(email, password));
    }

    /// <inheritdoc/>
    public async Task<IFirebaseUser> SignInWithEmailLinkAsync(string email, string link)
    {
        var authResult = await FirebaseAuthExceptionFactory.Wrap(
            () => _firebaseAuth.SignInWithLinkAsync(email, link)
        );
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    /// <inheritdoc/>
    public async Task<IFirebaseUser> SignInAnonymouslyAsync()
    {
        var authResult = await FirebaseAuthExceptionFactory.Wrap(
            () => _firebaseAuth.SignInAnonymouslyAsync()
        );
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    /// <inheritdoc/>
    public async Task<IFirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(
        string verificationCode
    )
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

        var authResult = await FirebaseAuthExceptionFactory.Wrap(() => currentUser.LinkAsync(credential));
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    /// <inheritdoc/>
    public async Task<IFirebaseUser> LinkWithEmailAndPasswordAsync(string email, string password)
    {
        var credential = await _emailAuth.GetCredentialAsync(email, password);
        return await LinkWithCredentialAsync(credential);
    }

    /// <inheritdoc/>
    public async Task SendSignInLink(string toEmail, CrossActionCodeSettings actionCodeSettings)
    {
        var nativeActionCodeSettings = actionCodeSettings.ToNative();
        if(nativeActionCodeSettings is null) {
            throw new InvalidOperationException("ActionCodeSettings.ToNative() returned null.");
        }

        await FirebaseAuthExceptionFactory.Wrap(
            () => _firebaseAuth.SendSignInLinkAsync(toEmail, nativeActionCodeSettings)
        );
    }

    /// <inheritdoc/>
    public Task SignOutAsync()
    {
        _firebaseAuth.SignOut(out var error);

        return error is null
            ? Task.CompletedTask
            : throw FirebaseAuthExceptionFactory.Create(new NSErrorException(error));
    }

    /// <inheritdoc/>
    public bool IsSignInWithEmailLink(string link)
    {
        try {
            return _firebaseAuth.IsSignIn(link);
        } catch(NSErrorException e) {
            throw FirebaseAuthExceptionFactory.Create(e);
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
            : FirebaseAuthExceptionFactory.Wrap(() => _firebaseAuth.SendPasswordResetAsync(email));
    }

    /// <inheritdoc/>
    public Task SendPasswordResetEmailAsync(string email)
    {
        return FirebaseAuthExceptionFactory.Wrap(() => _firebaseAuth.SendPasswordResetAsync(email));
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

        await FirebaseAuthExceptionFactory.Wrap(() => currentUser.ReloadAsync());
    }

    /// <inheritdoc/>
    public string? LanguageCode {
        set => _firebaseAuth.LanguageCode = value;
    }

    /// <inheritdoc/>
    public void UseAppLanguage()
    {
        _firebaseAuth.UseAppLanguage();
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
        return new DisposableWithAction(() => _firebaseAuth.RemoveAuthStateDidChangeListener(handle));
    }

    private static bool ShouldAttemptCreateUser(CrossPlatformFirebaseAuthException exception)
    {
        return exception.NativeErrorCode is
            nameof(AuthErrorCode.UserNotFound) or
            nameof(AuthErrorCode.InvalidCredential);
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

    /// <inheritdoc/>
    public IFirebaseUser? CurrentUser => _firebaseAuth.CurrentUser?.ToAbstract();
}
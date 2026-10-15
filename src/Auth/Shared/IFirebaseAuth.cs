namespace Plugin.Firebase.Auth;

/// <summary>
/// Manages authentication for Firebase apps.
/// </summary>
public partial interface IFirebaseAuth : IDisposable
{
    /// <summary>
    /// Starts the phone number authentication flow by sending a verification code to the specified phone number.
    /// </summary>
    /// <param name="phoneNumber">The phone number to be verified.</param>
    Task VerifyPhoneNumberAsync(string phoneNumber);

    /// <summary>
    /// Tries to create a new user account with the given email address and password. If successful, it also signs the user in into the app.
    /// </summary>
    /// <param name="email">The user’s email address.</param>
    /// <param name="password">The user’s password.</param>
    /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
    Task<IFirebaseUser> CreateUserAsync(string email, string password);

    /// <summary>
    /// Asynchronously signs in to Firebase with the given Auth token.
    /// </summary>
    /// <param name="token">A self-signed custom auth token.</param>
    /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
    Task<IFirebaseUser> SignInWithCustomTokenAsync(string token);

    /// <summary>
    /// Signs in using the verification code that was send to the given phone number after calling <c>VerifyPhoneNumberAsync(phoneNumber)</c>.
    /// </summary>
    /// <param name="verificationCode">The code that was send to the given phone number after calling <c>VerifyPhoneNumberAsync(phoneNumber)</c>.</param>
    /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
    Task<IFirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode);

    /// <summary>
    /// Signs in using an email address and password.
    /// </summary>
    /// <param name="email">The user’s email address.</param>
    /// <param name="password">The user’s password.</param>
    /// <param name="createsUserAutomatically">If the user doesn't exist, it will be created automatically and signed in afterwards if the value is true.</param>
    /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
    /// <exception cref="Plugin.Firebase.Core.Exceptions.CrossPlatformFirebaseAuthException">
    /// Thrown when sign-in fails and no account is created. The remarks describe which native error is thrown.
    /// </exception>
    /// <remarks>
    /// <para>
    /// With <paramref name="createsUserAutomatically"/> set to true, account creation is attempted only when sign-in fails
    /// with <c>ERROR_USER_NOT_FOUND</c> or <c>ERROR_INVALID_CREDENTIAL</c> (Android) or <c>UserNotFound</c> or
    /// <c>InvalidCredential</c> (iOS). With email enumeration protection, Firebase reports a missing account as an
    /// invalid credential, so a wrong password for an existing account also leads to an attempt. Other sign-in errors,
    /// such as a wrong password reported as <c>ERROR_WRONG_PASSWORD</c> / <c>WrongPassword</c>, a disabled user or a
    /// disabled email/password provider, are thrown without an attempt.
    /// </para>
    /// <para>
    /// When the attempt fails, the original sign-in error is thrown if creation reports that the email is already in use
    /// (<c>ERROR_EMAIL_ALREADY_IN_USE</c> / <c>EmailAlreadyInUse</c>) or that the project doesn't allow creating accounts
    /// from the client (<c>ERROR_ADMIN_RESTRICTED_OPERATION</c> / <c>AdminRestrictedOperation</c>, for example when user
    /// sign-up is disabled in Identity Platform, or <c>ERROR_OPERATION_NOT_ALLOWED</c> / <c>OperationNotAllowed</c>).
    /// Any other creation error is thrown instead of the sign-in error, for example <c>ERROR_WEAK_PASSWORD</c> /
    /// <c>WeakPassword</c>, an invalid email, throttling, a network error, or a sign-up rejected by a
    /// <c>beforeUserCreated</c> blocking function (an internal error on Android, <c>BlockingCloudFunctionError</c> on iOS).
    /// </para>
    /// <para>
    /// The thrown error can therefore show whether an email is registered, even with email enumeration protection
    /// enabled. For example, on the Auth emulator, which checks for an existing account before the password length, a
    /// too-short password gives a weak-password error for an unregistered email and the sign-in error for a registered
    /// one. <see cref="CreateUserAsync"/> already reveals this through its email-already-in-use error.
    /// </para>
    /// <para>
    /// Apps that only sign in existing users should pass false, so that only sign-in errors are thrown.
    /// </para>
    /// </remarks>
    Task<IFirebaseUser> SignInWithEmailAndPasswordAsync(
        string email,
        string password,
        bool createsUserAutomatically = true
    );

    /// <summary>
    /// Signs in using an email address and email sign-in link.
    /// </summary>
    /// <param name="email">The user’s email address.</param>
    /// <param name="link">The email sign-in link.</param>
    /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
    Task<IFirebaseUser> SignInWithEmailLinkAsync(string email, string link);

    /// <summary>
    /// Asynchronously creates and becomes an anonymous user. If there is already an anonymous user signed in, that user will
    /// be returned instead. If there is any other existing user signed in, that user will be signed out.
    /// </summary>
    /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
    Task<IFirebaseUser> SignInAnonymouslyAsync();

    /// <summary>
    /// Link the signed in user with a phone number.
    /// </summary>
    /// <param name="verificationCode">The code that was send to the given phone number after calling <c>VerifyPhoneNumberAsync(phoneNumber)</c>.</param>
    /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
    Task<IFirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(string verificationCode);

    /// <summary>
    /// Link the signed in user with the given email address and password.
    /// </summary>
    /// <param name="email">The user’s email address.</param>
    /// <param name="password">The user’s password.</param>
    /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
    Task<IFirebaseUser> LinkWithEmailAndPasswordAsync(string email, string password);

    /// <summary>
    /// Sends a sign in with email link to provided email address.
    /// </summary>
    /// <param name="toEmail">The email address of the user.</param>
    /// <param name="actionCodeSettings">An <c>ActionCodeSettings</c> object containing settings related to handling action codes.</param>
    Task SendSignInLink(string toEmail, ActionCodeSettings actionCodeSettings);

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    Task SignOutAsync();

    /// <summary>
    /// Checks if link is an email sign-in link.
    /// </summary>
    /// <param name="link">The email sign-in link.</param>
    bool IsSignInWithEmailLink(string link);

    /// <summary>
    /// Sends authenticated user an email with a link to reset his password.
    /// </summary>
    Task SendPasswordResetEmailAsync();

    /// <summary>
    /// Sends registered user an email with a link to reset his password.
    /// </summary>
    /// <param name="email">The registered user email.</param>
    Task SendPasswordResetEmailAsync(string email);

    /// <summary>
    /// Reloads the currently signed in user from the backend.
    /// </summary>
    /// <exception cref="Plugin.Firebase.Core.Exceptions.FirebaseException">Thrown when no user is signed in.</exception>
    [Obsolete("Use CurrentUser.ReloadAsync() after checking CurrentUser is not null.")]
    Task ReloadCurrentUserAsync();

    /// <summary>
    /// Sets the language used by Firebase Auth for user-facing flows such as Auth-generated emails/SMS (password reset, email verification, phone auth).
    /// Call this before invoking an API that triggers the flow, then reset by calling <see cref="UseAppLanguage"/>.
    /// </summary>
    /// <param name="value">A BCP-47 language code (e.g. "fr", "en-GB"), or null to clear.</param>
    string? LanguageCode { set; }

    /// <summary>
    /// Uses the app language for Firebase Auth user-facing flows.
    /// </summary>
    void UseAppLanguage();

    /// <summary>
    /// Modify this FirebaseAuth instance to communicate with the Firebase Authentication emulator.
    /// Note: this must be called before this instance has been used to do any operations.
    /// </summary>
    /// <param name="host">The emulator host (for example, 10.0.2.2 on android and localhost on iOS)</param>
    /// <param name="port">The emulator port (for example, 9099)</param>
    void UseEmulator(string host, int port);

    /// <summary>
    /// Registers a listener for authentication state changes.
    /// </summary>
    /// <param name="listener">Action invoked.</param>
    /// <returns>A token that detaches the listener when disposed.</returns>
    IDisposable AddAuthStateListener(Action<IFirebaseAuth> listener);

    /// <summary>
    /// The currently signed in <c>IFirebaseUser</c> object or <c>null</c> if the user is signed out.
    /// </summary>
    IFirebaseUser? CurrentUser { get; }
}
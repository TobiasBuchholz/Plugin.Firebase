namespace Plugin.Firebase.Auth;

/// <summary>
/// Manages authentication for Firebase apps.
/// </summary>
public partial interface IFirebaseAuth : IDisposable
{
    /// <summary>
    /// Starts the phone number authentication flow by sending a verification code to the specified phone number.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The returned task completes when the verification code has been sent, so a following
    /// <see cref="SignInWithPhoneNumberVerificationCodeAsync"/> or <see cref="LinkWithPhoneNumberVerificationCodeAsync"/>
    /// call can use it. The task faults with <see cref="Plugin.Firebase.Core.Exceptions.CrossPlatformFirebaseAuthException"/>
    /// when verification fails.
    /// </para>
    /// <para>
    /// Android: a repeat call for a phone number that already received a code passes the native resend token, so
    /// Firebase starts a new verification and sends a new code, as a "Resend" button expects. Until the new code is
    /// sent, sign-in keeps using the previous code. A call for the same number while the previous call is still pending
    /// returns that call's task. A call for a different number cancels the task of an earlier call that is still
    /// pending, and sign-in then uses only the new number's verification.
    /// </para>
    /// <para>
    /// Android: before it sends the code, the native SDK runs app verification, which can show a reCAPTCHA challenge.
    /// The native SDK can drop the callbacks if the activity stops before the code is sent, for example when the user
    /// switches to another app. If the native SDK reports no sent code, completion or failure within five minutes of the
    /// call, the task faults with <see cref="TimeoutException"/>; call this method again. Apply a shorter timeout of
    /// your own if your UI needs one.
    /// </para>
    /// <para>
    /// Android: the task also completes if Firebase verifies the number without sending a code. In firebase-auth 23.x
    /// that only happens for test numbers set up with the native
    /// <c>FirebaseAuthSettings.setAutoRetrievedSmsCodeForPhoneNumber</c> setting; in production a code is always sent.
    /// Sign-in and link then use that credential and ignore their verification code until the next call to this method.
    /// </para>
    /// <para>
    /// Android delivers the verification callbacks on the main thread. Await the task instead of blocking on it (for
    /// example with <c>Wait()</c> or <c>Result</c>) from the UI thread, which deadlocks.
    /// </para>
    /// </remarks>
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
    /// <remarks>
    /// Android: uses the verification of the latest <see cref="VerifyPhoneNumberAsync"/> call. While a repeat call for
    /// the same number is pending, or after it failed or timed out, the previous code for that number is used. If the
    /// latest call completed without sending a code (only for natively configured test numbers), this uses that
    /// credential and ignores <paramref name="verificationCode"/>.
    /// </remarks>
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
    /// <remarks>
    /// Android: uses the verification of the latest <see cref="VerifyPhoneNumberAsync"/> call. While a repeat call for
    /// the same number is pending, or after it failed or timed out, the previous code for that number is used. If the
    /// latest call completed without sending a code (only for natively configured test numbers), this uses that
    /// credential and ignores <paramref name="verificationCode"/>.
    /// </remarks>
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
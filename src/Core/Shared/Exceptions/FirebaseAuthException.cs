namespace Plugin.Firebase.Core.Exceptions;

public enum FIRAuthError
{
    /// <summary>
    /// Unknown error reason.
    /// </summary>
    Undefined,
    /// <summary>
    /// Indicates the email address is malformed.
    /// </summary>
    InvalidEmail,
    /// <summary>
    /// Indicates the user attempted sign in with a wrong password.
    /// </summary>
    WrongPassword,
    /// <summary>
    /// Indicates an attempt to set a password that is considered too weak.
    /// </summary>
    WeakPassword,
    /// <summary>
    /// Indicates the email used to attempt sign up already exists.
    /// </summary>
    EmailAlreadyInUse,
    /// <summary>
    /// Indicates the user account was not found.
    /// </summary>
    UserNotFound,
    /// <summary>
    /// Indicates the current user’s token has expired, for example, the user may have changed account password on another device.
    /// You must prompt the user to sign in again on this device.
    /// </summary>
    UserTokenExpired,
    /// <summary>
    /// Indicates the supplied credential is invalid. This could happen if it has expired or it is malformed.
    /// </summary>
    InvalidCredential,
    /// <summary>
    /// Indicates the user's account is disabled.
    /// </summary>
    UserDisabled,
    /// <summary>
    /// Indicates the user already signed in once with a trusted provider and hence, cannot sign in with untrusted provider anymore.
    /// List of trusted and untrusted providers: https://firebase.google.com/docs/auth/users#verified_email_addresses
    /// </summary>
    AccountExistsWithDifferentCredential
}

public class FirebaseAuthException : FirebaseException
{
    public FIRAuthError Reason { get; }

    public FirebaseAuthException(FIRAuthError reason)
    {
        Reason = reason;
    }

    public FirebaseAuthException(FIRAuthError reason, string message)
        : base(message)
    {
        Reason = reason;
    }

    public FirebaseAuthException(FIRAuthError reason, string message, Exception inner)
        : base(message, inner)
    {
        Reason = reason;
    }
}
namespace Plugin.Firebase.Core.Exceptions;

/// <summary>
/// Best-effort, non-exhaustive classifications for common Firebase Auth failures.
/// </summary>
public enum FirebaseAuthFailure
{
    /// <summary>
    /// The supplied credentials are invalid, malformed, or expired.
    /// </summary>
    InvalidCredentials,

    /// <summary>
    /// The requested user account was not found.
    /// </summary>
    UserNotFound,

    /// <summary>
    /// The requested operation collided with an existing user account.
    /// </summary>
    UserCollision,

    /// <summary>
    /// The supplied password is considered too weak.
    /// </summary>
    WeakPassword,

    /// <summary>
    /// The operation requires a more recent sign-in.
    /// </summary>
    RequiresRecentLogin,

    /// <summary>
    /// The request was throttled by Firebase.
    /// </summary>
    TooManyRequests
}

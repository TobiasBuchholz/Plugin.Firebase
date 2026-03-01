namespace Plugin.Firebase.Core.Exceptions;

/// <summary>
/// Platforms supported by Firebase Auth in Plugin.Firebase.
/// </summary>
public enum FirebaseAuthPlatform
{
    /// <summary>
    /// Android Firebase Auth SDK.
    /// </summary>
    Android,

    /// <summary>
    /// iOS Firebase Auth SDK.
    /// </summary>
    iOS
}

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

/// <summary>
/// Well-known Android Firebase Auth exception type names.
/// </summary>
public static class FirebaseAuthAndroidExceptionTypeNames
{
    /// <summary>
    /// <c>com.google.firebase.auth.FirebaseAuthEmailException</c>
    /// </summary>
    public const string EmailException = "com.google.firebase.auth.FirebaseAuthEmailException";

    /// <summary>
    /// <c>com.google.firebase.auth.FirebaseAuthInvalidCredentialsException</c>
    /// </summary>
    public const string InvalidCredentialsException =
        "com.google.firebase.auth.FirebaseAuthInvalidCredentialsException";

    /// <summary>
    /// <c>com.google.firebase.auth.FirebaseAuthInvalidUserException</c>
    /// </summary>
    public const string InvalidUserException =
        "com.google.firebase.auth.FirebaseAuthInvalidUserException";

    /// <summary>
    /// <c>com.google.firebase.auth.FirebaseAuthRecentLoginRequiredException</c>
    /// </summary>
    public const string RecentLoginRequiredException =
        "com.google.firebase.auth.FirebaseAuthRecentLoginRequiredException";

    /// <summary>
    /// <c>com.google.firebase.auth.FirebaseAuthUserCollisionException</c>
    /// </summary>
    public const string UserCollisionException =
        "com.google.firebase.auth.FirebaseAuthUserCollisionException";

    /// <summary>
    /// <c>com.google.firebase.auth.FirebaseAuthWeakPasswordException</c>
    /// </summary>
    public const string WeakPasswordException =
        "com.google.firebase.auth.FirebaseAuthWeakPasswordException";
}

/// <summary>
/// Cross-platform wrapper for native Firebase Auth failures.
/// </summary>
public sealed class CrossPlatformFirebaseAuthException : FirebaseException
{
    /// <summary>
    /// Creates a new instance wrapping a native Firebase Auth exception.
    /// </summary>
    /// <param name="platform">The originating native platform.</param>
    /// <param name="nativeExceptionTypeName">The native exception type name.</param>
    /// <param name="nativeErrorMessage">The native error message.</param>
    /// <param name="innerException">The original native exception.</param>
    /// <param name="nativeErrorDomain">The native error domain, if available.</param>
    /// <param name="nativeErrorCode">The native error code, if available.</param>
    public CrossPlatformFirebaseAuthException(
        FirebaseAuthPlatform platform,
        string nativeExceptionTypeName,
        string? nativeErrorMessage,
        Exception innerException,
        string? nativeErrorDomain = null,
        string? nativeErrorCode = null
    )
        : base(nativeErrorMessage ?? innerException.Message, innerException)
    {
        ArgumentNullException.ThrowIfNull(nativeExceptionTypeName);
        ArgumentNullException.ThrowIfNull(innerException);

        Platform = platform;
        NativeExceptionTypeName = nativeExceptionTypeName;
        NativeErrorDomain = nativeErrorDomain;
        NativeErrorCode = nativeErrorCode;
        NativeErrorMessage = nativeErrorMessage ?? innerException.Message;
    }

    /// <summary>
    /// The platform that produced the native Firebase Auth failure.
    /// </summary>
    public FirebaseAuthPlatform Platform { get; }

    /// <summary>
    /// The native exception type name.
    /// </summary>
    public string NativeExceptionTypeName { get; }

    /// <summary>
    /// The native error domain, when available.
    /// </summary>
    public string? NativeErrorDomain { get; }

    /// <summary>
    /// The native error code, when available.
    /// </summary>
    public string? NativeErrorCode { get; }

    /// <summary>
    /// The native error message.
    /// </summary>
    public string NativeErrorMessage { get; }
}

/// <summary>
/// Best-effort classification helper for <see cref="CrossPlatformFirebaseAuthException"/>.
/// </summary>
public static class FirebaseAuthErrorClassifier
{
    private const string FirebaseTooManyRequestsExceptionTypeName =
        "com.google.firebase.FirebaseTooManyRequestsException";

    /// <summary>
    /// Attempts to classify a Firebase Auth failure.
    /// </summary>
    /// <param name="exception">The wrapped Firebase Auth exception.</param>
    /// <returns>A best-effort classification, or <see langword="null"/> when unclassified.</returns>
    public static FirebaseAuthFailure? TryClassify(CrossPlatformFirebaseAuthException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception.Platform switch {
            FirebaseAuthPlatform.Android => TryClassifyAndroid(exception),
            FirebaseAuthPlatform.iOS => TryClassifyiOS(exception),
            _ => null
        };
    }

    private static FirebaseAuthFailure? TryClassifyAndroid(
        CrossPlatformFirebaseAuthException exception
    )
    {
        return exception.NativeExceptionTypeName switch {
            FirebaseAuthAndroidExceptionTypeNames.InvalidCredentialsException =>
                FirebaseAuthFailure.InvalidCredentials,
            FirebaseAuthAndroidExceptionTypeNames.InvalidUserException =>
                FirebaseAuthFailure.UserNotFound,
            FirebaseAuthAndroidExceptionTypeNames.UserCollisionException =>
                FirebaseAuthFailure.UserCollision,
            FirebaseAuthAndroidExceptionTypeNames.WeakPasswordException =>
                FirebaseAuthFailure.WeakPassword,
            FirebaseAuthAndroidExceptionTypeNames.RecentLoginRequiredException =>
                FirebaseAuthFailure.RequiresRecentLogin,
            FirebaseTooManyRequestsExceptionTypeName => FirebaseAuthFailure.TooManyRequests,
            _ => null
        };
    }

    private static FirebaseAuthFailure? TryClassifyiOS(CrossPlatformFirebaseAuthException exception)
    {
        return exception.NativeErrorCode switch {
            "InvalidCredential" => FirebaseAuthFailure.InvalidCredentials,
            "UserNotFound" => FirebaseAuthFailure.UserNotFound,
            "EmailAlreadyInUse" => FirebaseAuthFailure.UserCollision,
            "AccountExistsWithDifferentCredential" => FirebaseAuthFailure.UserCollision,
            "WeakPassword" => FirebaseAuthFailure.WeakPassword,
            "RequiresRecentLogin" => FirebaseAuthFailure.RequiresRecentLogin,
            "TooManyRequests" => FirebaseAuthFailure.TooManyRequests,
            _ => null
        };
    }
}

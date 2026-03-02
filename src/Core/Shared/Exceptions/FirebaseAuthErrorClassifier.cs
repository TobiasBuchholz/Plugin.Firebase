namespace Plugin.Firebase.Core.Exceptions;

/// <summary>
/// Best-effort classification helper for <see cref="CrossPlatformFirebaseAuthException"/>.
/// </summary>
public static class FirebaseAuthErrorClassifier
{
    private const string AndroidTooManyRequestsExceptionTypeName =
        "com.google.firebase.FirebaseTooManyRequestsException";

    /// <summary>
    /// Attempts to classify a Firebase Auth failure.
    /// </summary>
    /// <param name="exception">The wrapped Firebase Auth exception.</param>
    /// <returns>A best-effort classification, or <see langword="null"/> when unclassified.</returns>
    public static FirebaseAuthFailure? TryClassify(CrossPlatformFirebaseAuthException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception.NativeErrorDomain is null
            ? TryClassifyAndroid(exception.NativeExceptionTypeName)
            : TryClassifyiOS(exception.NativeErrorCode);
    }

    private static FirebaseAuthFailure? TryClassifyAndroid(string nativeExceptionTypeName)
    {
        return nativeExceptionTypeName switch {
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
            AndroidTooManyRequestsExceptionTypeName => FirebaseAuthFailure.TooManyRequests,
            _ => null
        };
    }

    private static FirebaseAuthFailure? TryClassifyiOS(string? nativeErrorCode)
    {
        return nativeErrorCode switch {
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
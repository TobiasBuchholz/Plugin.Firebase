namespace Plugin.Firebase.Core.Exceptions;

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
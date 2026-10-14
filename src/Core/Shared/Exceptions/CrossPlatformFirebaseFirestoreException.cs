namespace Plugin.Firebase.Core.Exceptions;

/// <summary>
/// Cross-platform wrapper for native Firebase Firestore failures.
/// </summary>
/// <remarks>
/// Plugin.Firebase.Firestore surfaces every failure the native SDK reports as a Firebase exception
/// (Android) or an <c>NSError</c> (iOS) as this type, with the native exception as
/// <see cref="Exception.InnerException"/>. The native metadata is passed through:
/// <list type="bullet">
/// <item><description>
/// Android: <see cref="CrossPlatformFirebaseException.NativeExceptionTypeName"/> is the Java canonical
/// class name, for example <c>com.google.firebase.firestore.FirebaseFirestoreException</c>;
/// <see cref="CrossPlatformFirebaseException.NativeErrorCode"/> is the
/// <c>FirebaseFirestoreException.Code</c> name, for example <c>INVALID_ARGUMENT</c>; and
/// <see cref="CrossPlatformFirebaseException.NativeErrorDomain"/> is <see langword="null"/>.
/// </description></item>
/// <item><description>
/// iOS: <see cref="CrossPlatformFirebaseException.NativeExceptionTypeName"/> is <c>NSError</c>;
/// <see cref="CrossPlatformFirebaseException.NativeErrorDomain"/> is the <c>NSError</c> domain, for example
/// <c>FIRFirestoreErrorDomain</c>; and <see cref="CrossPlatformFirebaseException.NativeErrorCode"/> is the
/// <c>FirestoreErrorCode</c> name, for example <c>InvalidArgument</c>, or the numeric code formatted with
/// the invariant culture when the domain is not the Firestore error domain or the code has no name.
/// </description></item>
/// </list>
/// </remarks>
public sealed class CrossPlatformFirebaseFirestoreException : CrossPlatformFirebaseException
{
    /// <summary>
    /// Creates a new instance wrapping a native Firebase Firestore exception.
    /// </summary>
    /// <param name="nativeExceptionTypeName">The native exception type name.</param>
    /// <param name="nativeErrorMessage">The native error message.</param>
    /// <param name="innerException">The original native exception.</param>
    /// <param name="nativeErrorDomain">The native error domain, if available.</param>
    /// <param name="nativeErrorCode">The native error code, if available.</param>
    public CrossPlatformFirebaseFirestoreException(
        string nativeExceptionTypeName,
        string? nativeErrorMessage,
        Exception innerException,
        string? nativeErrorDomain = null,
        string? nativeErrorCode = null
    )
        : base(
            nativeExceptionTypeName,
            nativeErrorMessage,
            innerException,
            nativeErrorDomain,
            nativeErrorCode
        )
    {
    }
}
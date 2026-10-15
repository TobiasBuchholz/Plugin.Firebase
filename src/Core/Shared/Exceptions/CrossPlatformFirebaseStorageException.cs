namespace Plugin.Firebase.Core.Exceptions;

/// <summary>
/// Cross-platform wrapper for native Firebase Storage failures.
/// </summary>
/// <remarks>
/// Plugin.Firebase.Storage surfaces every failure the native SDK reports as a Firebase exception (Android) or an
/// <c>NSError</c> (iOS) as this type, with the native exception as <see cref="Exception.InnerException"/>. The native
/// metadata is passed through:
/// <list type="bullet">
/// <item><description>
/// Android: <see cref="CrossPlatformFirebaseException.NativeExceptionTypeName"/> is the Java canonical class name, for
/// example <c>com.google.firebase.storage.StorageException</c>; <see cref="CrossPlatformFirebaseException.NativeErrorCode"/>
/// is <c>StorageException.ErrorCode</c>, for example <c>-13010</c> for an object that doesn't exist;
/// <see cref="NativeHttpResultCode"/> is <c>StorageException.HttpResultCode</c>; and
/// <see cref="CrossPlatformFirebaseException.NativeErrorDomain"/> is <see langword="null"/>. Both codes are
/// <see langword="null"/> for a Firebase exception that isn't a <c>StorageException</c>.
/// </description></item>
/// <item><description>
/// iOS: <see cref="CrossPlatformFirebaseException.NativeExceptionTypeName"/> is <c>NSError</c>;
/// <see cref="CrossPlatformFirebaseException.NativeErrorDomain"/> is the <c>NSError</c> domain, for example
/// <c>FIRStorageErrorDomain</c>; <see cref="CrossPlatformFirebaseException.NativeErrorCode"/> is the <c>NSError</c>
/// code, for example <c>-13010</c> (<c>StorageErrorCode.objectNotFound</c>); and <see cref="NativeHttpResultCode"/> is
/// the HTTP status of the failed request, when there is one.
/// </description></item>
/// </list>
/// <see cref="CrossPlatformFirebaseException.NativeErrorCode"/> is formatted with the invariant culture, so it reads
/// the same in every locale.
/// </remarks>
public sealed class CrossPlatformFirebaseStorageException : CrossPlatformFirebaseException
{
    /// <summary>
    /// Creates a new instance wrapping a native Firebase Storage exception.
    /// </summary>
    /// <param name="nativeExceptionTypeName">The native exception type name.</param>
    /// <param name="nativeErrorMessage">The native error message.</param>
    /// <param name="innerException">The original native exception.</param>
    /// <param name="nativeErrorDomain">The native error domain, if available.</param>
    /// <param name="nativeErrorCode">The native error code, if available.</param>
    /// <param name="nativeHttpResultCode">The HTTP result code of the failed request, if
    /// available.</param>
    public CrossPlatformFirebaseStorageException(
        string nativeExceptionTypeName,
        string? nativeErrorMessage,
        Exception innerException,
        string? nativeErrorDomain = null,
        string? nativeErrorCode = null,
        int? nativeHttpResultCode = null
    )
        : base(
            nativeExceptionTypeName,
            nativeErrorMessage,
            innerException,
            nativeErrorDomain,
            nativeErrorCode
        )
    {
        NativeHttpResultCode = nativeHttpResultCode;
    }

    /// <summary>
    /// The HTTP result code of the failed request, when available.
    /// </summary>
    /// <remarks>
    /// On Android this is <c>StorageException.HttpResultCode</c> as the native SDK reports it, which is a value outside
    /// the HTTP status range, such as <c>0</c>, when the failure didn't come from an HTTP response. On iOS it is the
    /// HTTP status that FirebaseStorage records in the <c>NSError</c> user info (<c>ResponseErrorCode</c>, when
    /// <c>ResponseErrorDomain</c> is <c>com.google.HTTPStatus</c>), and <see langword="null"/> when the request didn't
    /// fail with an HTTP status.
    /// </remarks>
    public int? NativeHttpResultCode { get; }
}
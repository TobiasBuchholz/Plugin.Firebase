namespace Plugin.Firebase.Core.Exceptions;

/// <summary>
/// Cross-platform wrapper for native Firebase Storage failures.
/// </summary>
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
    /// The HTTP result code of the failed request, when available. The native Android SDK uses
    /// -1 when no HTTP request was made.
    /// </summary>
    public int? NativeHttpResultCode { get; }
}
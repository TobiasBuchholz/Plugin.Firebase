namespace Plugin.Firebase.Core.Exceptions;

/// <summary>
/// Cross-platform wrapper for native Firebase Auth failures.
/// </summary>
public sealed class CrossPlatformFirebaseAuthException : FirebaseException
{
    /// <summary>
    /// Creates a new instance wrapping a native Firebase Auth exception.
    /// </summary>
    /// <param name="nativeExceptionTypeName">The native exception type name.</param>
    /// <param name="nativeErrorMessage">The native error message.</param>
    /// <param name="innerException">The original native exception.</param>
    /// <param name="nativeErrorDomain">The native error domain, if available.</param>
    /// <param name="nativeErrorCode">The native error code, if available.</param>
    public CrossPlatformFirebaseAuthException(
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

        NativeExceptionTypeName = nativeExceptionTypeName;
        NativeErrorDomain = nativeErrorDomain;
        NativeErrorCode = nativeErrorCode;
        NativeErrorMessage = nativeErrorMessage ?? innerException.Message;
    }

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
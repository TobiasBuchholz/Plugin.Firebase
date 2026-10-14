namespace Plugin.Firebase.Core.Exceptions;

/// <summary>
/// Base type for cross-platform wrappers of native Firebase failures. Keeps the native exception
/// as <see cref="Exception.InnerException"/> together with the native type name, domain and code,
/// so shared code can catch one type on both platforms without losing native detail.
/// </summary>
public class CrossPlatformFirebaseException : FirebaseException
{
    /// <summary>
    /// Creates a new instance wrapping a native Firebase exception.
    /// </summary>
    /// <param name="nativeExceptionTypeName">The native exception type name.</param>
    /// <param name="nativeErrorMessage">The native error message.</param>
    /// <param name="innerException">The original native exception.</param>
    /// <param name="nativeErrorDomain">The native error domain, if available.</param>
    /// <param name="nativeErrorCode">The native error code, if available.</param>
    public CrossPlatformFirebaseException(
        string nativeExceptionTypeName,
        string? nativeErrorMessage,
        Exception innerException,
        string? nativeErrorDomain = null,
        string? nativeErrorCode = null
    )
        : base(ResolveMessage(nativeErrorMessage, innerException), innerException)
    {
        ArgumentNullException.ThrowIfNull(nativeExceptionTypeName);

        NativeExceptionTypeName = nativeExceptionTypeName;
        NativeErrorDomain = nativeErrorDomain;
        NativeErrorCode = nativeErrorCode;
        NativeErrorMessage = nativeErrorMessage ?? innerException.Message;
    }

    // Runs before the base constructor, so a null inner exception fails with ArgumentNullException
    // instead of a NullReferenceException from reading its message.
    private static string ResolveMessage(string? nativeErrorMessage, Exception innerException)
    {
        ArgumentNullException.ThrowIfNull(innerException);
        return nativeErrorMessage ?? innerException.Message;
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
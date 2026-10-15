using System.Globalization;
using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.Storage;

/// <summary>
/// Wraps native Firebase Storage failures in <see cref="CrossPlatformFirebaseStorageException"/>. The platform parts
/// decide which failures are native and read their metadata; this part builds the exception.
/// </summary>
internal static partial class StorageExceptionFactory
{
    /// <summary>
    /// Creates the cross-platform exception from native metadata. The native error code is formatted with the
    /// invariant culture, so a negative Storage code such as -13010 reads the same in every locale instead of, for
    /// example, using a U+2212 minus sign in sv-SE.
    /// </summary>
    internal static CrossPlatformFirebaseStorageException Create(
        string nativeExceptionTypeName,
        string? nativeErrorMessage,
        Exception innerException,
        string? nativeErrorDomain,
        long? nativeErrorCode,
        int? nativeHttpResultCode
    )
    {
        return new CrossPlatformFirebaseStorageException(
            nativeExceptionTypeName,
            nativeErrorMessage,
            innerException,
            nativeErrorDomain,
            nativeErrorCode?.ToString(CultureInfo.InvariantCulture),
            nativeHttpResultCode
        );
    }
}
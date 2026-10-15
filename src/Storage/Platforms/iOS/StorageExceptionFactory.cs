using System.Globalization;
using Firebase.Storage;
using Foundation;
using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.Storage;

/// <summary>
/// iOS part: wraps <see cref="NSError"/> failures. The type name is <c>NSError</c>, the error domain is the
/// <see cref="NSError"/> domain, and the error code is the numeric <see cref="NSError"/> code. The HTTP result code is
/// read from the user info that FirebaseStorage attaches to errors from a failed request, and is
/// <see langword="null"/> when the request didn't fail with an HTTP status.
/// </summary>
internal static partial class StorageExceptionFactory
{
    // FirebaseStorage's StorageErrorDomain. The binding exposes StorageErrorCode but not the domain constant.
    private const string StorageErrorDomain = "FIRStorageErrorDomain";

    // FirebaseStorage's StorageErrorCode.error(withServerError:ref:) copies the domain and code of the failed fetch's
    // error into these user-info keys. GTMSessionFetcher reports an HTTP status as an error in the
    // com.google.HTTPStatus domain (kGTMSessionFetcherStatusDomain) whose code is the status.
    private const string ResponseErrorDomainKey = "ResponseErrorDomain";
    private const string ResponseErrorCodeKey = "ResponseErrorCode";
    private const string HttpStatusErrorDomain = "com.google.HTTPStatus";

    public static bool IsNativeStorageException(Exception exception)
    {
        return exception is NSErrorException;
    }

    public static CrossPlatformFirebaseStorageException Create(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if(exception is not NSErrorException nsErrorException) {
            throw new ArgumentException("Expected a native Firebase exception.", nameof(exception));
        }

        var error = nsErrorException.Error;
        return Create(
            nameof(NSError),
            error.LocalizedDescription ?? nsErrorException.Message,
            nsErrorException,
            error.Domain,
            (long) error.Code,
            GetHttpResultCode(error)
        );
    }

    public static CrossPlatformFirebaseStorageException Create(NSError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return Create(new NSErrorException(error));
    }

    /// <summary>
    /// Wraps <paramref name="exception"/> when it is a native <see cref="NSErrorException"/> and returns any other
    /// exception unchanged.
    /// </summary>
    public static Exception WrapIfNative(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return IsNativeStorageException(exception) ? Create(exception) : exception;
    }

    /// <summary>
    /// Creates the <c>downloadSizeExceeded</c> error (<c>-13032</c> in <c>FIRStorageErrorDomain</c>) that FirebaseStorage
    /// reports when an object is larger than the requested maximum size. Used when the native SDK completes with more
    /// data than the maximum anyway, so that case fails with the same metadata as the native size check.
    /// </summary>
    public static CrossPlatformFirebaseStorageException CreateDownloadSizeExceeded(long totalSize, long maxSize)
    {
        var description = string.Create(
            CultureInfo.InvariantCulture,
            $"Attempted to download object with size of {totalSize} bytes, which exceeds the maximum size of {maxSize} bytes."
        );
        var userInfo = NSDictionary.FromObjectsAndKeys(
            new NSObject[] { new NSString(description), NSNumber.FromInt64(totalSize), NSNumber.FromInt64(maxSize) },
            new NSObject[] { NSError.LocalizedDescriptionKey, new NSString("totalSize"), new NSString("maxAllowedSize") }
        );

        return Create(
            new NSError(new NSString(StorageErrorDomain), (nint) StorageErrorCode.DownloadSizeExceeded, userInfo)
        );
    }

    public static async Task Wrap(Func<Task> operation)
    {
        try {
            await operation();
        } catch(Exception exception) when(IsNativeStorageException(exception)) {
            throw Create(exception);
        }
    }

    public static async Task<T> Wrap<T>(Func<Task<T>> operation)
    {
        try {
            return await operation();
        } catch(Exception exception) when(IsNativeStorageException(exception)) {
            throw Create(exception);
        }
    }

    private static int? GetHttpResultCode(NSError error)
    {
        var userInfo = error.UserInfo;
        if(userInfo?[ResponseErrorDomainKey] is not NSString responseErrorDomain
            || responseErrorDomain.ToString() != HttpStatusErrorDomain
            || userInfo[ResponseErrorCodeKey] is not NSNumber responseErrorCode) {
            return null;
        }

        return responseErrorCode.Int32Value;
    }
}
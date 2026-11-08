using Plugin.Firebase.Core.Exceptions;
using JavaThrowable = Java.Lang.Throwable;
using NativeFirebaseException = Firebase.FirebaseException;
using NativeStorageException = Firebase.Storage.StorageException;

namespace Plugin.Firebase.Storage;

/// <summary>
/// Android part: wraps native Firebase exceptions. The type name is the Java canonical class name, the error code is
/// <see cref="NativeStorageException.ErrorCode"/> and the HTTP result code is
/// <see cref="NativeStorageException.HttpResultCode"/>, both passed through verbatim, and the error domain is
/// <see langword="null"/>.
/// </summary>
internal static partial class StorageExceptionFactory
{
    public static bool IsNativeStorageException(Exception exception)
    {
        return exception is NativeFirebaseException;
    }

    public static CrossPlatformFirebaseStorageException Create(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if(exception is not NativeFirebaseException nativeException) {
            throw new ArgumentException("Expected a native Firebase exception.", nameof(exception));
        }

        var storageException = nativeException as NativeStorageException;
        return Create(
            GetNativeExceptionTypeName(nativeException),
            nativeException.Message,
            nativeException,
            nativeErrorDomain: null,
            nativeErrorCode: storageException?.ErrorCode,
            nativeHttpResultCode: storageException?.HttpResultCode
        );
    }

    /// <summary>
    /// Wraps <paramref name="exception"/> when it is a native Firebase exception and returns any other exception
    /// unchanged.
    /// </summary>
    public static Exception WrapIfNative(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return IsNativeStorageException(exception) ? Create(exception) : exception;
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

    private static string GetNativeExceptionTypeName(JavaThrowable throwable)
    {
        var canonicalName = throwable.Class?.CanonicalName;
        return string.IsNullOrEmpty(canonicalName)
            ? throwable.GetType().FullName ?? throwable.GetType().Name
            : canonicalName;
    }
}
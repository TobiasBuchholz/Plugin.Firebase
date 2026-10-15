using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.IntegrationTests.Storage;

internal static class StorageErrorAssertions
{
    private const string AndroidStorageExceptionTypeName = "com.google.firebase.storage.StorageException";
    private const string AndroidStorageExceptionManagedTypeName = "Firebase.Storage.StorageException";
    private const string IosStorageErrorDomain = "FIRStorageErrorDomain";

    // StorageException.ERROR_OBJECT_NOT_FOUND on Android and StorageErrorCode.objectNotFound on iOS.
    public static void ObjectNotFound(Exception? exception)
    {
        var storageException = NativeStorageFailure(exception);
        Assert.Equal("-13010", storageException.NativeErrorCode);
        Assert.Equal(404, storageException.NativeHttpResultCode);
    }

    // Android fails the native size check with ERROR_UNKNOWN; iOS reports StorageErrorCode.downloadSizeExceeded.
    public static void DownloadSizeExceeded(Exception? exception)
    {
        var storageException = NativeStorageFailure(exception);
        if(OperatingSystem.IsAndroid()) {
            Assert.Equal("-13000", storageException.NativeErrorCode);
        } else {
            Assert.Equal("-13032", storageException.NativeErrorCode);
            Assert.Null(storageException.NativeHttpResultCode);
        }
    }

    private static CrossPlatformFirebaseStorageException NativeStorageFailure(Exception? exception)
    {
        var storageException = Assert.IsType<CrossPlatformFirebaseStorageException>(exception);
        if(OperatingSystem.IsAndroid()) {
            Assert.Equal(AndroidStorageExceptionTypeName, storageException.NativeExceptionTypeName);
            Assert.Null(storageException.NativeErrorDomain);
            Assert.Equal(
                AndroidStorageExceptionManagedTypeName,
                storageException.InnerException?.GetType().FullName);
        } else {
            Assert.Equal("NSError", storageException.NativeExceptionTypeName);
            Assert.Equal(IosStorageErrorDomain, storageException.NativeErrorDomain);
            Assert.Equal("Foundation.NSErrorException", storageException.InnerException?.GetType().FullName);
        }
        Assert.False(string.IsNullOrEmpty(storageException.NativeErrorMessage));
        return storageException;
    }
}
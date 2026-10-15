using Firebase.Storage;
using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.Storage.Platforms.Android;

public sealed class StorageTaskTaskSnapshotWrapper : IStorageTaskSnapshot
{
    public static IStorageTaskSnapshot FromSnapshot(StorageTask.SnapshotBase snapshot)
    {
        switch(snapshot) {
            case UploadTask.TaskSnapshot x:
                return new StorageTaskTaskSnapshotWrapper(x);
            case FileDownloadTask.TaskSnapshot x:
                return new StorageTaskTaskSnapshotWrapper(x);
            default:
                throw new FirebaseException($"Couldn't wrap unsupported StorageTask.SnapshotBase {snapshot}");
        }
    }

    /// <summary>
    /// Creates a failure snapshot. A native Firebase exception, such as the <c>StorageException</c> that a failed
    /// transfer reports, is wrapped in <see cref="CrossPlatformFirebaseStorageException"/>; any other exception is kept
    /// as it is.
    /// </summary>
    /// <param name="error">The exception the transfer failed with.</param>
    /// <returns>A snapshot whose <see cref="IStorageTaskSnapshot.Error"/> is the wrapped exception.</returns>
    public static IStorageTaskSnapshot FromError(Exception error)
    {
        return new StorageTaskTaskSnapshotWrapper(error: StorageExceptionFactory.WrapIfNative(error));
    }

    private StorageTaskTaskSnapshotWrapper(
        UploadTask.TaskSnapshot? snapshot = null,
        Exception? error = null)
        : this(error)
    {
        if(snapshot != null) {
            TransferredUnitCount = snapshot.BytesTransferred;
            TotalUnitCount = snapshot.TotalByteCount;
            TransferredFraction = snapshot.BytesTransferred / (double) snapshot.TotalByteCount;
        }

        Metadata = snapshot?.Metadata?.ToAbstract();
    }

    private StorageTaskTaskSnapshotWrapper(
        FileDownloadTask.TaskSnapshot? snapshot = null,
        Exception? error = null)
        : this(error)
    {
        if(snapshot != null) {
            TransferredUnitCount = snapshot.BytesTransferred;
            TotalUnitCount = snapshot.TotalByteCount;
            TransferredFraction = snapshot.BytesTransferred / (double) snapshot.TotalByteCount;
        }
    }

    private StorageTaskTaskSnapshotWrapper(Exception? error = null)
    {
        Error = error;
    }

    public long TransferredUnitCount { get; }
    public long TotalUnitCount { get; }
    public double TransferredFraction { get; }
    public IStorageMetadata? Metadata { get; }
    public Exception? Error { get; }
}
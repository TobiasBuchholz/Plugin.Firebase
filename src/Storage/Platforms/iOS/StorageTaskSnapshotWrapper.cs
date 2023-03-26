using Firebase.Storage;
using Plugin.Firebase.Storage.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Storage.Platforms.iOS;

public class StorageTaskTaskSnapshotWrapper : IStorageTaskSnapshot
{
    public static IStorageTaskSnapshot FromSnapshot(StorageTaskSnapshot snapshot)
    {
        return new StorageTaskTaskSnapshotWrapper(snapshot: snapshot);
    }

    public static IStorageTaskSnapshot FromError(Exception error)
    {
        return new StorageTaskTaskSnapshotWrapper(error: error);
    }

    private StorageTaskTaskSnapshotWrapper(
        StorageTaskSnapshot snapshot = null,
        Exception error = null)
    {
        if(snapshot?.Progress != null) {
            TransferredUnitCount = snapshot.Progress.CompletedUnitCount;
            TotalUnitCount = snapshot.Progress.TotalUnitCount;
            TransferredFraction = snapshot.Progress.FractionCompleted;
        }

        Metadata = snapshot?.Metadata?.ToAbstract();
        Error = error;
    }

    public long TransferredUnitCount { get; }
    public long TotalUnitCount { get; }
    public double TransferredFraction { get; }
    public IStorageMetadata Metadata { get; }
    public Exception Error { get; }
}
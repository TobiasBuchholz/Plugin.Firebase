using Firebase.Storage;
using Plugin.Firebase.Storage.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Storage.Platforms.iOS;

/// <summary>
/// Wraps a native iOS Firebase StorageTaskSnapshot to implement IStorageTaskSnapshot.
/// </summary>
public class StorageTaskTaskSnapshotWrapper : IStorageTaskSnapshot
{
    /// <summary>
    /// Creates a new snapshot wrapper from a native iOS storage task snapshot.
    /// </summary>
    /// <param name="snapshot">The native snapshot to wrap.</param>
    /// <returns>An abstract storage task snapshot.</returns>
    public static IStorageTaskSnapshot FromSnapshot(StorageTaskSnapshot snapshot)
    {
        return new StorageTaskTaskSnapshotWrapper(snapshot: snapshot);
    }

    /// <summary>
    /// Creates a new snapshot wrapper representing an error state.
    /// </summary>
    /// <param name="error">The exception that occurred.</param>
    /// <returns>An abstract storage task snapshot with the error.</returns>
    public static IStorageTaskSnapshot FromError(Exception error)
    {
        return new StorageTaskTaskSnapshotWrapper(error: error);
    }

    private StorageTaskTaskSnapshotWrapper(
        StorageTaskSnapshot snapshot = null,
        Exception error = null
    )
    {
        if(snapshot?.Progress != null) {
            TransferredUnitCount = snapshot.Progress.CompletedUnitCount;
            TotalUnitCount = snapshot.Progress.TotalUnitCount;
            TransferredFraction = snapshot.Progress.FractionCompleted;
        }

        Metadata = snapshot?.Metadata?.ToAbstract();
        Error = error;
    }

    /// <inheritdoc/>
    public long TransferredUnitCount { get; }

    /// <inheritdoc/>
    public long TotalUnitCount { get; }

    /// <inheritdoc/>
    public double TransferredFraction { get; }

    /// <inheritdoc/>
    public IStorageMetadata Metadata { get; }

    /// <inheritdoc/>
    public Exception Error { get; }
}
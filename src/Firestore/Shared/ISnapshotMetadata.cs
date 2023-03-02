namespace Plugin.Firebase.Firestore;

/// <summary>
/// Metadata about a snapshot, describing the state of the snapshot.
/// </summary>
public interface ISnapshotMetadata
{
    /// <summary>
    /// Returns <c>true</c> if the snapshot contains the result of local writes (e.g. set() or update() calls) that have not yet been committed
    /// to the backend. If your listener has opted into metadata updates (via includeMetadataChanges:true) you will receive another snapshot
    /// with hasPendingWrites equal to false once the writes have been committed to the backend.
    /// </summary>
    bool HasPendingWrites { get; }
    
    /// <summary>
    /// Returns <c>true</c> if the snapshot was created from cached data rather than guaranteed up-to-date server data. If your listener has opted
    /// into metadata updates you will receive another snapshot with <c>IsFromCache</c> equal to false once the client has received up-to-date data
    /// from the backend.
    /// </summary>
    bool IsFromCache { get; }
}
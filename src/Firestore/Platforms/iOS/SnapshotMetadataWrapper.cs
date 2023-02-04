using Firebase.CloudFirestore;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.iOS.Firestore;

public sealed class SnapshotMetadataWrapper : ISnapshotMetadata
{
    public SnapshotMetadataWrapper(SnapshotMetadata metadata)
    {
        HasPendingWrites = metadata.HasPendingWrites;
        IsFromCache = metadata.IsFromCache;
    }

    public bool HasPendingWrites { get; }
    public bool IsFromCache { get; }
}
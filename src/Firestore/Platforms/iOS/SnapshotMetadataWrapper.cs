using Firebase.CloudFirestore;

namespace Plugin.Firebase.Firestore.Platforms.iOS;

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
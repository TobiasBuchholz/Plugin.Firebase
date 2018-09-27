using Firebase.CloudFirestore;
using Plugin.Firebase.Abstractions.Firestore;

namespace Plugin.Firebase.iOS.Firestore
{
    public sealed class SnapshotMetadataWrapper : ISnapshotMetadata
    {
        public SnapshotMetadataWrapper(SnapshotMetadata metadata)
        {
            HasPendingWrites = metadata.HasPendingWrites;
        }

        public bool HasPendingWrites { get; }
    }
}
using Firebase.Firestore;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.Android.Firestore
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
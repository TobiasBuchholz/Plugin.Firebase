namespace Plugin.Firebase.Firestore
{
    public interface ISnapshotMetadata
    {
        bool HasPendingWrites { get; }
    }
}
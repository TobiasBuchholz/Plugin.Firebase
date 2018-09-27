namespace Plugin.Firebase.Abstractions.Firestore
{
    public interface ISnapshotMetadata
    {
        bool HasPendingWrites { get; }
    }
}
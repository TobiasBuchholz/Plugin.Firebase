namespace Plugin.Firebase.Firestore
{
    public interface IDocumentSnapshot<out T>
    {
        T Data { get; }
        ISnapshotMetadata Metadata { get; }
        IDocumentReference Reference { get; }
    }
}
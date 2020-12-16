namespace Plugin.Firebase.Firestore
{
    public interface IDocumentSnapshot<out T> : IDocumentSnapshot
    {
        T Data { get; }
        ISnapshotMetadata Metadata { get; }
        IDocumentReference Reference { get; }
    }

    public interface IDocumentSnapshot
    {
    }
}
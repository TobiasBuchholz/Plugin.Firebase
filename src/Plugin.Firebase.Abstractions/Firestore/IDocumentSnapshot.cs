namespace Plugin.Firebase.Abstractions.Firestore
{
    public interface IDocumentSnapshot<T>
    {
        T Data { get; }
        ISnapshotMetadata Metadata { get; }
        IDocumentReference Reference { get; }
    }
}
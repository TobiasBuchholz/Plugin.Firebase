using Firebase.CloudFirestore;
using Plugin.Firebase.Firestore.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Firestore.Platforms.iOS;

public sealed class DocumentSnapshotWrapper<T> : DocumentSnapshotWrapper, IDocumentSnapshot<T>
{
    public DocumentSnapshotWrapper(DocumentSnapshot documentSnapshot)
        : base(documentSnapshot)
    {
    }

    public new T Data => Wrapped.Data == null ? default(T) : Wrapped.Data.Cast<T>(Wrapped.Id);
}

public class DocumentSnapshotWrapper : IDocumentSnapshot
{
    public DocumentSnapshotWrapper(DocumentSnapshot snapshot)
    {
        Wrapped = snapshot;
    }

    public object Data => Wrapped.Data;
    public ISnapshotMetadata Metadata => Wrapped.Metadata.ToAbstract();
    public IDocumentReference Reference => Wrapped.Reference.ToAbstract();
    public DocumentSnapshot Wrapped { get; }
}
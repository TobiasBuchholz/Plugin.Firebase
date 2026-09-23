using Firebase.Firestore;
using Plugin.Firebase.Firestore.Platforms.Android.Extensions;

namespace Plugin.Firebase.Firestore.Platforms.Android;

public sealed class DocumentSnapshotWrapper<T> : DocumentSnapshotWrapper, IDocumentSnapshot<T>
{
    private readonly Lazy<T?> _data;

    public DocumentSnapshotWrapper(DocumentSnapshot documentSnapshot)
        : base(documentSnapshot)
    {
        _data = new Lazy<T?>(ConvertData);
    }

    public new T? Data => _data.Value;

    private T? ConvertData()
    {
        var data = Wrapped.Data;
        return data == null ? default(T) : data.Cast<T>(Wrapped.Id);
    }
}

public class DocumentSnapshotWrapper : IDocumentSnapshot
{
    public DocumentSnapshotWrapper(DocumentSnapshot snapshot)
    {
        Wrapped = snapshot;
    }

    public object? Data => Wrapped.Data;
    public ISnapshotMetadata Metadata => Wrapped.Metadata.ToAbstract();
    public IDocumentReference Reference => Wrapped.Reference.ToAbstract();
    public DocumentSnapshot Wrapped { get; }
}
using Firebase.Firestore;
using Plugin.Firebase.Firestore.Platforms.Android.Extensions;

namespace Plugin.Firebase.Firestore.Platforms.Android;

public sealed class DocumentSnapshotWrapper<T> : DocumentSnapshotWrapper, IDocumentSnapshot<T>
{
    private ConvertedData<T>? _data;

    public DocumentSnapshotWrapper(DocumentSnapshot documentSnapshot)
        : base(documentSnapshot)
    {
    }

    public new T? Data => ConvertedData<T>.GetOrConvert(ref _data, Wrapped, ConvertData);

    private static T? ConvertData(DocumentSnapshot snapshot)
    {
        var data = snapshot.Data;
        return data == null ? default(T) : data.Cast<T>(snapshot.Id);
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